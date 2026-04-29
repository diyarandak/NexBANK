using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IExchangeService
{
    Task<Dictionary<string, decimal>> GetExchangeRatesAsync();
    Task<bool> ExchangeCurrencyAsync(int userId, int fromAccountId, int toAccountId, decimal amount, string toCurrency);
}
