using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface ICreditCardRepository
{
    Task<CreditCard?> GetByIdAsync(int id);
    Task<List<CreditCard>> GetByUserIdAsync(int userId);
    Task<List<CreditCard>> GetAllAsync();
    Task<CreditCard> AddAsync(CreditCard creditCard);
    Task UpdateAsync(CreditCard creditCard);
    Task AddTransactionAsync(CreditCardTransaction transaction);
}
