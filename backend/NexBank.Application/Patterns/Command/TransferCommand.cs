using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Patterns.Command;

public class TransferCommand : ITransactionCommand
{
    private readonly IAccountRepository _accountRepository;
    private readonly Transaction _transaction;
    private readonly decimal _fee;

    public TransferCommand(
        IAccountRepository accountRepository, 
        Transaction transaction, 
        decimal fee)
    {
        _accountRepository = accountRepository;
        _transaction = transaction;
        _fee = fee;
    }

    public async Task<bool> ExecuteAsync()
    {
        var fromAccount = await _accountRepository.GetByIdAsync(_transaction.FromAccountId!.Value);
        var toAccount = await _accountRepository.GetByIdAsync(_transaction.ToAccountId!.Value);

        if (fromAccount == null || toAccount == null) return false;

        decimal totalAmount = _transaction.Amount + _fee;

        if (fromAccount.Balance < totalAmount)
            return false;

        fromAccount.Balance -= totalAmount;
        toAccount.Balance += _transaction.Amount;

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);

        _transaction.Status = TransactionStatus.Approved;
        _transaction.ProcessedAt = DateTime.UtcNow;

        return true;
    }

    public async Task<bool> UndoAsync()
    {
        // Personelin hem onaylanmış hem de bekleyen (şüpheli) işlemleri reddedebilmesi için:
        if (_transaction.Status != TransactionStatus.Approved && _transaction.Status != TransactionStatus.Pending)
            return false;

        var fromAccount = await _accountRepository.GetByIdAsync(_transaction.FromAccountId!.Value);
        var toAccount = await _accountRepository.GetByIdAsync(_transaction.ToAccountId!.Value);

        if (fromAccount == null || toAccount == null) return false;

        decimal totalAmount = _transaction.Amount + _fee;

        fromAccount.Balance += totalAmount; // Kesintiyi ve tutarı iade et
        toAccount.Balance -= _transaction.Amount; // Karşıdan geri al

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);

        _transaction.Status = TransactionStatus.Rejected; // Veya Canceled/Reverted için yeni enum eklenebilir.
        _transaction.Description += " [Geri Alındı]";

        return true;
    }

    public int GetTransactionId() => _transaction.Id;
}
