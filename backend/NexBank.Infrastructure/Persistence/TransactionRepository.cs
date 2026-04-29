using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class TransactionRepository : ITransactionRepository
{
    private readonly NexBankDbContext _context;

    public TransactionRepository(NexBankDbContext context) => _context = context;

    public async Task<Transaction?> GetByIdAsync(int id) =>
        await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Transaction>> GetByAccountIdAsync(int accountId) =>
        await _context.Transactions
            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<Transaction>> GetAllAsync() =>
        await _context.Transactions.OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<List<Transaction>> GetPendingAsync() =>
        await _context.Transactions
            .Where(t => t.Status == TransactionStatus.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync() =>
        await _context.Transactions.CountAsync();

    public async Task<int> GetTodayCountAsync() =>
        await _context.Transactions.CountAsync(t => t.CreatedAt.Date == DateTime.UtcNow.Date);

    public async Task<decimal> GetTodayVolumeAsync() =>
        await _context.Transactions
            .Where(t => t.CreatedAt.Date == DateTime.UtcNow.Date)
            .SumAsync(t => t.Amount);
}
