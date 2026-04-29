using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class AccountRepository : IAccountRepository
{
    private readonly NexBankDbContext _context;

    public AccountRepository(NexBankDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(int id) =>
        await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber) =>
        await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<Account?> GetByIbanAsync(string iban) =>
        await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Iban == iban);

    public async Task<List<Account>> GetByUserIdAsync(int userId) =>
        await _context.Accounts.Include(a => a.User).Where(a => a.UserId == userId).ToListAsync();

    public async Task<List<Account>> GetAllAsync() =>
        await _context.Accounts.Include(a => a.User).ToListAsync();

    public async Task<Account> AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync() =>
        await _context.Accounts.CountAsync(a => a.IsActive);

    public async Task<decimal> GetTotalBalanceAsync() =>
        await _context.Accounts.Where(a => a.IsActive).SumAsync(a => a.Balance);
}
