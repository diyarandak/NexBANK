using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class CreditCardRepository : ICreditCardRepository
{
    private readonly NexBankDbContext _context;

    public CreditCardRepository(NexBankDbContext context) => _context = context;

    public async Task<CreditCard?> GetByIdAsync(int id) =>
        await _context.CreditCards
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<CreditCard>> GetByUserIdAsync(int userId) =>
        await _context.CreditCards
            .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt))
            .Where(c => c.UserId == userId)
            .ToListAsync();

    public async Task<List<CreditCard>> GetAllAsync() =>
        await _context.CreditCards.ToListAsync();

    public async Task<CreditCard> AddAsync(CreditCard creditCard)
    {
        _context.CreditCards.Add(creditCard);
        await _context.SaveChangesAsync();
        return creditCard;
    }

    public async Task UpdateAsync(CreditCard creditCard)
    {
        _context.CreditCards.Update(creditCard);
        await _context.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(CreditCardTransaction transaction)
    {
        _context.CreditCardTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
}
