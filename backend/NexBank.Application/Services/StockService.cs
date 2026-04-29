using NexBank.Application.Interfaces;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private static readonly Random _random = new();

    public StockService(
        IStockRepository stockRepository, 
        IAccountRepository accountRepository, 
        ITransactionRepository transactionRepository)
    {
        _stockRepository = stockRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<List<Stock>> GetAllStocksAsync()
    {
        var stocks = await _stockRepository.GetAllAsync();
        if (!stocks.Any())
        {
            stocks = new List<Stock>
            {
                new() { Symbol = "THYAO", Name = "Türk Hava Yolları", CurrentPrice = 285.50m, OpeningPrice = 284.00m, Sector = "Ulaşım" },
                new() { Symbol = "ASELS", Name = "Aselsan", CurrentPrice = 62.20m, OpeningPrice = 61.50m, Sector = "Savunma" },
                new() { Symbol = "EREGL", Name = "Erdemir", CurrentPrice = 48.10m, OpeningPrice = 49.00m, Sector = "Demir Çelik" },
                new() { Symbol = "ASTOR", Name = "Astor Enerji", CurrentPrice = 95.30m, OpeningPrice = 92.00m, Sector = "Enerji" },
                new() { Symbol = "KOZAL", Name = "Koza Altın", CurrentPrice = 22.40m, OpeningPrice = 22.80m, Sector = "Madencilik" },
            };
            await _stockRepository.AddStocksAsync(stocks);
        }
        return stocks;
    }

    public async Task<List<UserStock>> GetUserPortfolioAsync(int userId)
    {
        return await _stockRepository.GetUserPortfolioAsync(userId);
    }

    public async Task<bool> BuyStockAsync(int userId, int accountId, string symbol, decimal quantity)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        var stock = await _stockRepository.GetBySymbolAsync(symbol);

        if (account == null || stock == null || account.UserId != userId) return false;

        decimal totalCost = stock.CurrentPrice * quantity;
        if (account.Balance < totalCost) return false;

        account.Balance -= totalCost;
        await _accountRepository.UpdateAsync(account);

        var userStock = await _stockRepository.GetUserStockAsync(userId, symbol);
        if (userStock == null)
        {
            userStock = new UserStock { UserId = userId, StockSymbol = symbol, Quantity = quantity, AverageCost = stock.CurrentPrice };
            await _stockRepository.AddUserStockAsync(userStock);
        }
        else
        {
            decimal currentTotalValue = userStock.Quantity * userStock.AverageCost;
            userStock.Quantity += quantity;
            userStock.AverageCost = (currentTotalValue + totalCost) / userStock.Quantity;
            await _stockRepository.UpdateUserStockAsync(userStock);
        }

        await _transactionRepository.AddAsync(new Transaction
        {
            FromAccountId = accountId,
            Amount = totalCost,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Approved,
            Description = $"Hisse Alımı: {quantity} Lot {symbol} @ {stock.CurrentPrice}₺",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        });

        return true;
    }

    public async Task<bool> SellStockAsync(int userId, int accountId, string symbol, decimal quantity)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        var stock = await _stockRepository.GetBySymbolAsync(symbol);
        var userStock = await _stockRepository.GetUserStockAsync(userId, symbol);

        if (account == null || stock == null || userStock == null || account.UserId != userId) return false;
        if (userStock.Quantity < quantity) return false;

        decimal totalGain = stock.CurrentPrice * quantity;
        
        userStock.Quantity -= quantity;
        if (userStock.Quantity == 0) await _stockRepository.RemoveUserStockAsync(userStock);
        else await _stockRepository.UpdateUserStockAsync(userStock);

        account.Balance += totalGain;
        await _accountRepository.UpdateAsync(account);

        await _transactionRepository.AddAsync(new Transaction
        {
            ToAccountId = accountId,
            Amount = totalGain,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Approved,
            Description = $"Hisse Satışı: {quantity} Lot {symbol} @ {stock.CurrentPrice}₺",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        });

        return true;
    }

    public async Task SimulateMarketAsync()
    {
        var stocks = await _stockRepository.GetAllAsync();
        foreach (var s in stocks)
        {
            decimal change = (decimal)(_random.NextDouble() * 0.04 - 0.02);
            s.CurrentPrice *= (1 + change);
            s.CurrentPrice = Math.Round(s.CurrentPrice, 2);
        }
        await _stockRepository.UpdateStocksAsync(stocks);
    }
}
