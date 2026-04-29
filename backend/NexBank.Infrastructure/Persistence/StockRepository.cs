using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;
using NexBank.Infrastructure.Persistence;

namespace NexBank.Infrastructure.Persistence;

public class StockRepository : IStockRepository
{
    private readonly NexBankDbContext _context;

    public StockRepository(NexBankDbContext context)
    {
        _context = context;
    }

    public async Task<List<Stock>> GetAllAsync() => await _context.Stocks.ToListAsync();

    public async Task<Stock?> GetBySymbolAsync(string symbol) => 
        await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

    public async Task<List<UserStock>> GetUserPortfolioAsync(int userId) =>
        await _context.UserStocks.Where(us => us.UserId == userId).ToListAsync();

    public async Task<UserStock?> GetUserStockAsync(int userId, string symbol) =>
        await _context.UserStocks.FirstOrDefaultAsync(us => us.UserId == userId && us.StockSymbol == symbol);

    public async Task AddUserStockAsync(UserStock userStock)
    {
        _context.UserStocks.Add(userStock);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserStockAsync(UserStock userStock)
    {
        _context.UserStocks.Update(userStock);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserStockAsync(UserStock userStock)
    {
        _context.UserStocks.Remove(userStock);
        await _context.SaveChangesAsync();
    }

    public async Task AddStocksAsync(IEnumerable<Stock> stocks)
    {
        _context.Stocks.AddRange(stocks);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStocksAsync(IEnumerable<Stock> stocks)
    {
        _context.Stocks.UpdateRange(stocks);
        await _context.SaveChangesAsync();
    }
}
