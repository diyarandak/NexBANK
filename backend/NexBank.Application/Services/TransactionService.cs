using NexBank.Application.Interfaces;
using NexBank.Application.Patterns.Command;
using NexBank.Application.Patterns.Strategy;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly TransactionInvoker _invoker;
    private readonly PaymentStrategyContext _strategyContext;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        TransactionInvoker invoker,
        PaymentStrategyContext strategyContext,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _invoker = invoker;
        _strategyContext = strategyContext;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    private void ValidateTransaction(Account account, decimal amount)
    {
        if (!account.IsActive)
            throw new Exception("Hesap pasif durumda. İşlem yapılamaz.");

        if (amount > account.DailyLimit)
            throw new Exception($"Günlük işlem limitini ({account.DailyLimit}₺) aştınız.");

        if (account.Balance < amount)
            throw new Exception("Yetersiz bakiye. İşlem reddedildi.");
    }

    public async Task<bool> MakeTransferAsync(int fromAccountId, string toIban, decimal amount, string paymentMethodStr)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var toAccount = await _accountRepository.GetByIbanAsync(toIban);
            if (toAccount == null)
                throw new ArgumentException("Alıcı hesap (IBAN) bulunamadı.");

            if (!Enum.TryParse<PaymentMethod>(paymentMethodStr, true, out var method))
                throw new ArgumentException("Geçersiz ödeme yöntemi.");

            decimal fee = _strategyContext.CalculateFee(method, amount);

            // Validasyon Kontrolleri
            var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
            if (fromAccount == null) throw new Exception("Gönderen hesap bulunamadı.");
            
            ValidateTransaction(fromAccount, amount + fee);

            // İşlem kaydını oluştur
            var transaction = new Transaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccount.Id,
                Amount = amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Pending,
                Description = $"{method} Transferi",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);

            // Masraf kaydı (Gönderen için ayrı satır)
            if (fee > 0)
            {
                var feeTx = new Transaction
                {
                    FromAccountId = fromAccountId,
                    Amount = fee,
                    Type = TransactionType.Withdrawal,
                    Status = amount >= 50000 ? TransactionStatus.Pending : TransactionStatus.Approved,
                    Description = $"{method} İşlem Ücreti",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(feeTx);
            }

            // 50.000 TL ve üstü ise PARAYI DÜŞME, sadece kaydet ve bekle
            if (amount >= 50000)
            {
                transaction.Status = TransactionStatus.Pending;
                transaction.Description += " [🚨 ŞÜPHELİ İŞLEM - PERSONEL ONAYI BEKLENİYOR]";
                await _transactionRepository.UpdateAsync(transaction); // Zaten AddAsync yapılmıştı, güncelle
                
                await _unitOfWork.CommitTransactionAsync();
                return true; // İşlem kaydedildi ama para transferi bekliyor
            }

            // 50.000 TL altı ise HEMEN gerçekleştir
            var transferCommand = new TransferCommand(_accountRepository, transaction, fee);
            var result = await _invoker.ExecuteCommandAsync(transferCommand, transaction);

            if (result)
            {
                await _unitOfWork.CommitTransactionAsync();
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
            }

            return result;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> MakeDepositAsync(int accountId, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Yatırılacak tutar pozitif olmalıdır.");

        var transaction = new Transaction
        {
            ToAccountId = accountId,
            Amount = amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            Description = "Para Yatırma",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);

        var depositCommand = new DepositCommand(_accountRepository, transaction);
        return await _invoker.ExecuteCommandAsync(depositCommand, transaction);
    }

    public async Task<bool> MakeWithdrawAsync(int accountId, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Çekilecek tutar pozitif olmalıdır.");

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null) throw new ArgumentException("Hesap bulunamadı.");
        
        // Validasyon Kontrolleri
        ValidateTransaction(account, amount);

        var transaction = new Transaction
        {
            FromAccountId = accountId,
            Amount = amount,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Pending,
            Description = "Para Çekme",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);

        var withdrawCommand = new WithdrawCommand(_accountRepository, transaction);
        return await _invoker.ExecuteCommandAsync(withdrawCommand, transaction);
    }

    public async Task<bool> UndoLastTransferAsync(int transactionId)
    {
        // 1. İşlemi bul
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.Status == TransactionStatus.Rejected) return false;

        // 2. EĞER İŞLEM HİÇ ONAYLANMAMIŞ VE 50K ÜSTÜYSE (PARA HİÇ ÇIKMADI)
        if (transaction.Status == TransactionStatus.Pending && transaction.Amount >= 50000)
        {
            transaction.Status = TransactionStatus.Rejected;
            transaction.Description += " [PERSONEL TARAFINDAN REDDEDİLDİ]";
            await _transactionRepository.UpdateAsync(transaction);

            // Masrafı da (Pending olan) iptal et
            var allTx = await _transactionRepository.GetAllAsync();
            var feeTx = allTx.FirstOrDefault(t => t.FromAccountId == transaction.FromAccountId && t.Type == TransactionType.Withdrawal && t.CreatedAt >= transaction.CreatedAt.AddSeconds(-5) && t.CreatedAt <= transaction.CreatedAt.AddSeconds(5));
            if (feeTx != null)
            {
                feeTx.Status = TransactionStatus.Rejected;
                await _transactionRepository.UpdateAsync(feeTx);
            }
            return true;
        }

        // 3. Önce Invoker/Command hafızasından dene (Hızlı iade)
        var success = await _invoker.UndoLastCommandAsync(transaction);
        
        // 3. Hafıza boşsa veya Command başarısızsa -> MANUEL KESİN İADE
        if (!success)
        {
            try 
            {
                var fromAcc = await _accountRepository.GetByIdAsync(transaction.FromAccountId ?? 0);
                var toAcc = await _accountRepository.GetByIdAsync(transaction.ToAccountId ?? 0);
                
                if (fromAcc != null && toAcc != null)
                {
                    // BAKİYE TAKASI (X'e iade, Y'den geri alış)
                    fromAcc.Balance += transaction.Amount; 
                    toAcc.Balance -= transaction.Amount;   
                    
                    transaction.Status = TransactionStatus.Rejected;
                    transaction.Description += " [İADE EDİLDİ]";
                    transaction.ProcessedAt = DateTime.UtcNow;
                    
                    // Veritabanını güncelle
                    await _accountRepository.UpdateAsync(fromAcc);
                    await _accountRepository.UpdateAsync(toAcc);
                    await _transactionRepository.UpdateAsync(transaction);

                    // Eğer bu transferin bir masrafı (EFT ücreti vb.) varsa onu da bul ve iptal et (İsteğe bağlı ama tutarlılık için iyi olur)
                    var allTransactions = await _transactionRepository.GetAllAsync();
                    var feeTx = allTransactions.FirstOrDefault(t => 
                        t.FromAccountId == fromAcc.Id && 
                        t.Type == TransactionType.Withdrawal && 
                        t.CreatedAt >= transaction.CreatedAt.AddSeconds(-5) && 
                        t.CreatedAt <= transaction.CreatedAt.AddSeconds(5) &&
                        (t.Description != null && t.Description.Contains("Ücreti")));
                    
                    if (feeTx != null && feeTx.Status != TransactionStatus.Rejected)
                    {
                        fromAcc.Balance += feeTx.Amount; // Masrafı da iade et
                        feeTx.Status = TransactionStatus.Rejected;
                        feeTx.Description += " [İADE]";
                        await _accountRepository.UpdateAsync(fromAcc);
                        await _transactionRepository.UpdateAsync(feeTx);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL UNDO ERROR] {ex.Message}");
                return false;
            }
        }

        return success;
    }

    public async Task<bool> ApproveTransactionAsync(int transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.Status != TransactionStatus.Pending) return false;

        var fromAccount = await _accountRepository.GetByIdAsync(transaction.FromAccountId ?? 0);
        var toAccount = await _accountRepository.GetByIdAsync(transaction.ToAccountId ?? 0);

        if (fromAccount == null || toAccount == null) return false;

        // Ücreti tekrar hesapla veya sistemden bul (EFT/Havale türüne göre)
        // Burada basitleştirmek için %0.4 - 80 formülünü EFT kabul ederek uyguluyoruz (veya genel CalculateFee)
        // Eğer transfer sırasında feeTx kaydedildiyse onu bulabiliriz.
        decimal fee = 0;
        var allTx = await _transactionRepository.GetAllAsync();
        var feeTx = allTx.FirstOrDefault(t => t.FromAccountId == fromAccount.Id && t.Type == TransactionType.Withdrawal && t.CreatedAt >= transaction.CreatedAt.AddSeconds(-5) && t.CreatedAt <= transaction.CreatedAt.AddSeconds(5));
        if (feeTx != null) fee = feeTx.Amount;

        // Bakiyeyi ŞİMDİ düşüyoruz
        if (fromAccount.Balance < (transaction.Amount + fee))
            throw new Exception("Hesap bakiyesi bu onay için artık yetersiz.");

        fromAccount.Balance -= (transaction.Amount + fee);
        toAccount.Balance += transaction.Amount;

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);

        transaction.Status = TransactionStatus.Approved;
        transaction.Description = transaction.Description?.Replace(" [🚨 ŞÜPHELİ İŞLEM - PERSONEL ONAYI BEKLENİYOR]", " [PERSONEL ONAYLI]");
        transaction.ProcessedAt = DateTime.UtcNow;
        
        await _transactionRepository.UpdateAsync(transaction);

        // Masraf işlemini de (eğer varsa) onaylı yapalım
        if (feeTx != null)
        {
            feeTx.Status = TransactionStatus.Approved;
            await _transactionRepository.UpdateAsync(feeTx);
        }

        return true;
    }

    public async Task<IEnumerable<NexBank.Application.DTOs.TransactionDto>> GetAllTransactionsAsync()
    {
        var all = await _transactionRepository.GetAllAsync();
        return all.OrderByDescending(t => t.CreatedAt).Select(t => new NexBank.Application.DTOs.TransactionDto
        {
            Id = t.Id,
            FromAccountId = t.FromAccountId,
            ToAccountId = t.ToAccountId,
            Amount = t.Amount,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Description = t.Description ?? "",
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<IEnumerable<NexBank.Application.DTOs.TransactionDto>> GetUserTransactionsAsync(int userId)
    {
        var userAccounts = await _accountRepository.GetByUserIdAsync(userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();
        
        var all = await _transactionRepository.GetAllAsync();
        var userTransactions = all.Where(t => 
            (t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value)) || 
            (t.ToAccountId.HasValue && accountIds.Contains(t.ToAccountId.Value))
        ).OrderByDescending(t => t.CreatedAt);

        return userTransactions.Select(t => new NexBank.Application.DTOs.TransactionDto
        {
            Id = t.Id,
            FromAccountId = t.FromAccountId,
            ToAccountId = t.ToAccountId,
            Amount = t.Amount,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Description = t.Description ?? "",
            CreatedAt = t.CreatedAt
        });
    }
}
