using Microsoft.Extensions.Caching.Memory;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

/// <summary>
/// DECORATOR PATTERN: IStockRepository'yi sarmalayarak Caching yeteneği ekler.
/// Mevcut StockRepository koduna dokunmadan performans artışı sağlar.
/// </summary>
public class CachedStockRepository : IStockRepository
{
    private readonly IStockRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private const string StocksCacheKey = "AllStocksCache";

    public CachedStockRepository(IStockRepository innerRepository, IMemoryCache cache)
    {
        _innerRepository = innerRepository;
        _cache = cache;
    }

    public async Task<List<Stock>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync(StocksCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromSeconds(10); // 10 saniye cache
            return _innerRepository.GetAllAsync();
        }) ?? new List<Stock>();
    }

    public async Task<Stock?> GetBySymbolAsync(string symbol)
    {
        string key = $"Stock_{symbol}";
        return await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return _innerRepository.GetBySymbolAsync(symbol);
        });
    }

    // Portfolio işlemleri kişiye özel olduğu için genellikle cache'lenmez veya farklı key ile cache'lenir.
    // Şimdilik direkt inner repository'ye paslıyoruz.
    public Task<List<UserStock>> GetUserPortfolioAsync(int userId) => _innerRepository.GetUserPortfolioAsync(userId);
    public Task<UserStock?> GetUserStockAsync(int userId, string symbol) => _innerRepository.GetUserStockAsync(userId, symbol);
    public Task AddUserStockAsync(UserStock userStock) => _innerRepository.AddUserStockAsync(userStock);
    public Task UpdateUserStockAsync(UserStock userStock) => _innerRepository.UpdateUserStockAsync(userStock);
    public Task RemoveUserStockAsync(UserStock userStock) => _innerRepository.RemoveUserStockAsync(userStock);
    public Task AddStocksAsync(IEnumerable<Stock> stocks) => _innerRepository.AddStocksAsync(stocks);
    
    public async Task UpdateStocksAsync(IEnumerable<Stock> stocks)
    {
        await _innerRepository.UpdateStocksAsync(stocks);
        _cache.Remove(StocksCacheKey); // Fiyatlar güncellendiğinde cache'i temizle
    }
}
