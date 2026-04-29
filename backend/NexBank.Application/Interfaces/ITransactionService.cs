using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface ITransactionService
{
    Task<bool> MakeTransferAsync(int fromAccountId, string toIban, decimal amount, string paymentMethodStr);
    Task<bool> MakeDepositAsync(int accountId, decimal amount);
    Task<bool> MakeWithdrawAsync(int accountId, decimal amount);
    Task<bool> UndoLastTransferAsync(int transactionId);
    Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
    Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(int userId);
}
