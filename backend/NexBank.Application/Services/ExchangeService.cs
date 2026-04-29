using NexBank.Application.Interfaces;
using NexBank.Core.Interfaces;
using NexBank.Core.Entities;

namespace NexBank.Application.Services;

public class ExchangeService : IExchangeService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    // Basit sabit kur tablosu (1 TRY baz alınarak)
    private readonly Dictionary<string, decimal> _mockRates = new Dictionary<string, decimal>
    {
        { "TRY", 1.0m },
        { "USD", 34.50m },
        { "EUR", 37.20m },
        { "GBP", 43.10m },
        { "XAU", 2560.00m } // Altın Gram
    };

    public ExchangeService(IAccountRepository accountRepository, ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
    {
        return Task.FromResult(_mockRates);
    }

    public async Task<bool> ExchangeCurrencyAsync(int userId, int fromAccountId, int toAccountId, decimal amountToExchange, string targetCurrency)
    {
        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
        var toAccount = await _accountRepository.GetByIdAsync(toAccountId);

        if (fromAccount == null || toAccount == null) return false;
        if (fromAccount.UserId != userId || toAccount.UserId != userId) return false;
        
        // Target account currency check
        if (toAccount.Currency != targetCurrency) return false;

        // Balance check
        if (fromAccount.Balance < amountToExchange) return false;

        // Exchange Rate Math (fromAccountCurrency -> TRY -> toAccountCurrency)
        decimal fromRate = _mockRates.ContainsKey(fromAccount.Currency) ? _mockRates[fromAccount.Currency] : 1.0m;
        decimal toRate = _mockRates.ContainsKey(targetCurrency) ? _mockRates[targetCurrency] : 1.0m;

        // Alış/Satış spread'i (banka kar payı, diyelim ki %1)
        decimal amountInTry = amountToExchange * fromRate; 
        decimal targetAmount = (amountInTry / toRate) * 0.99m; // %1 komisyon

        // İşlemleri gerçekleştir
        fromAccount.Balance -= amountToExchange;
        toAccount.Balance += targetAmount;

        var transaction = new Transaction
        {
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = amountToExchange,
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Approved,
            Description = $"Döviz İşlemi: {amountToExchange} {fromAccount.Currency} => {targetAmount:F2} {targetCurrency} (Kur Komisyonu Kesildi)",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        await _accountRepository.UpdateAsync(fromAccount);
        await _accountRepository.UpdateAsync(toAccount);
        await _transactionRepository.AddAsync(transaction);

        return true;
    }
}
