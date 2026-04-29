using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<Account?> GetByIbanAsync(string iban);
    Task<List<Account>> GetByUserIdAsync(int userId);
    Task<List<Account>> GetAllAsync();
    Task<Account> AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task<int> GetCountAsync();
    Task<decimal> GetTotalBalanceAsync();
}
