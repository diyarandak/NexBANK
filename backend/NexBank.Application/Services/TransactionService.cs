using NexBank.Application.Interfaces;
using NexBank.Application.Patterns.Command;
using NexBank.Application.Patterns.Strategy;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;
using NexBank.Application.Patterns.ChainOfResponsibility;

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

    private TransactionHandler GetValidationChain()
    {
        var status = new AccountStatusHandler();
        var limit = new DailyLimitHandler();
        var balance = new BalanceCheckHandler();

        status.SetNext(limit);
        limit.SetNext(balance);

        return status;
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

            // 0. CHAIN OF RESPONSIBILITY: Validasyon Zincirini Çalıştır
            var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
            if (fromAccount == null) throw new Exception("Gönderen hesap bulunamadı.");
            
            var chain = GetValidationChain();
            await chain.HandleAsync(fromAccount, amount + fee);

            // İşlem kaydını oluştur
            var transaction = new Transaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccount.Id,
                Amount = amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Pending,
                Description = $"{method} ile Transfer. Kesinti: {fee} ₺",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(transaction);

            // 2. COMMAND PATTERN: Komutu hazırla
            var transferCommand = new TransferCommand(_accountRepository, transaction, fee);

            // 3. EXECUTE & OBSERVE: Invoker çalıştıracak ve OBSERVER desenini tetikleyecek
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
        
        // CHAIN OF RESPONSIBILITY
        var chain = GetValidationChain();
        await chain.HandleAsync(account, amount);

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
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        return await _invoker.UndoLastCommandAsync(transaction);
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
