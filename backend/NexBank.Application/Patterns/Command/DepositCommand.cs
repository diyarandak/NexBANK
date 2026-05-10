using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Patterns.Command;

public class DepositCommand : ITransactionCommand
{
    private readonly IAccountRepository _accountRepository;
    private readonly Transaction _transaction;

    public DepositCommand(IAccountRepository accountRepository, Transaction transaction)
    {
        _accountRepository = accountRepository;
        _transaction = transaction;
    }

    public async Task<bool> ExecuteAsync()
    {
        var account = await _accountRepository.GetByIdAsync(_transaction.ToAccountId!.Value);
        if (account == null) return false;

        account.Balance += _transaction.Amount;
        await _accountRepository.UpdateAsync(account);

        _transaction.Status = TransactionStatus.Approved;
        _transaction.ProcessedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<bool> UndoAsync()
    {
        if (_transaction.Status != TransactionStatus.Approved) return false;

        var account = await _accountRepository.GetByIdAsync(_transaction.ToAccountId!.Value);
        if (account == null) return false;

        if (account.Balance < _transaction.Amount) return false; // Yetersiz bakiye (zaten harcamış)

        account.Balance -= _transaction.Amount;
        await _accountRepository.UpdateAsync(account);

        _transaction.Status = TransactionStatus.Rejected;
        _transaction.Description += " [Geri Alındı]";
        return true;
    }

    public int GetTransactionId() => _transaction.Id;
}
