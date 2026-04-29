using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface IStockRepository
{
    Task<List<Stock>> GetAllAsync();
    Task<Stock?> GetBySymbolAsync(string symbol);
    Task<List<UserStock>> GetUserPortfolioAsync(int userId);
    Task<UserStock?> GetUserStockAsync(int userId, string symbol);
    Task AddUserStockAsync(UserStock userStock);
    Task UpdateUserStockAsync(UserStock userStock);
    Task RemoveUserStockAsync(UserStock userStock);
    Task AddStocksAsync(IEnumerable<Stock> stocks);
    Task UpdateStocksAsync(IEnumerable<Stock> stocks);
}
