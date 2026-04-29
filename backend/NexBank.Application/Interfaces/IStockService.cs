using NexBank.Core.Entities;

namespace NexBank.Application.Interfaces;

public interface IStockService
{
    Task<List<Stock>> GetAllStocksAsync();
    Task<List<UserStock>> GetUserPortfolioAsync(int userId);
    Task<bool> BuyStockAsync(int userId, int accountId, string symbol, decimal quantity);
    Task<bool> SellStockAsync(int userId, int accountId, string symbol, decimal quantity);
    Task SimulateMarketAsync();
}
