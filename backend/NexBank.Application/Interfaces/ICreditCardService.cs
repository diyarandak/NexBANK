using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface ICreditCardService
{
    Task<List<CreditCardDto>> GetUserCreditCardsAsync(int userId);
    Task<CreditCardDto> CreateCreditCardAsync(int userId, decimal limit);
    Task<bool> SpendAsync(int userId, CreditCardSpendDto dto);
    Task<bool> PayDebtAsync(int userId, CreditCardPaymentDto dto);
    Task<bool> CashAdvanceAsync(int userId, CashAdvanceDto dto);
    Task<bool> UpdateLimitAsync(int userId, int cardId, decimal newLimit);
}
