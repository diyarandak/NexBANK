using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<List<Transaction>> GetByAccountIdAsync(int accountId);
    Task<List<Transaction>> GetAllAsync();
    Task<List<Transaction>> GetPendingAsync();
    Task<Transaction> AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<int> GetCountAsync();
    Task<int> GetTodayCountAsync();
    Task<decimal> GetTodayVolumeAsync();
}
