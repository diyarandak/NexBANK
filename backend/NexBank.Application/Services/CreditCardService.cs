using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ITransactionService _transactionService;
    private readonly IAccountRepository _accountRepository;

    public CreditCardService(ICreditCardRepository creditCardRepository, ITransactionService transactionService, IAccountRepository accountRepository)
    {
        _creditCardRepository = creditCardRepository;
        _transactionService = transactionService;
        _accountRepository = accountRepository;
    }

    public async Task<List<CreditCardDto>> GetUserCreditCardsAsync(int userId)
    {
        var cards = await _creditCardRepository.GetByUserIdAsync(userId);
        return cards.Select(MapToDto).ToList();
    }

    public async Task<CreditCardDto> CreateCreditCardAsync(int userId, decimal limit)
    {
        var random = new Random();
        var cardNo = "";
        for (int i = 0; i < 16; i++) cardNo += random.Next(0, 10).ToString();
        var cvv = random.Next(100, 999).ToString();
        var exp = $"{DateTime.UtcNow.AddYears(4).Month:D2}/{DateTime.UtcNow.AddYears(4).Year.ToString().Substring(2)}";

        var card = new CreditCard
        {
            UserId = userId,
            CardNumber = cardNo,
            Cvv = cvv,
            ExpiryDate = exp,
            CardLimit = limit,
            UsedAmount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _creditCardRepository.AddAsync(card);
        return MapToDto(saved);
    }

    public async Task<bool> SpendAsync(int userId, CreditCardSpendDto dto)
    {
        var card = await _creditCardRepository.GetByIdAsync(dto.CreditCardId);
        if (card == null || card.UserId != userId || !card.IsActive) return false;

        if (card.AvailableLimit < dto.Amount)
            throw new ArgumentException("Kredi kartı limiti yetersiz.");

        card.UsedAmount += dto.Amount;
        await _creditCardRepository.UpdateAsync(card);

        var transaction = new CreditCardTransaction
        {
            CreditCardId = card.Id,
            Amount = dto.Amount,
            Description = dto.Description,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow
        };
        await _creditCardRepository.AddTransactionAsync(transaction);

        return true;
    }

    public async Task<bool> PayDebtAsync(int userId, CreditCardPaymentDto dto)
    {
        var card = await _creditCardRepository.GetByIdAsync(dto.CreditCardId);
        if (card == null || card.UserId != userId || !card.IsActive) return false;

        var account = await _accountRepository.GetByIdAsync(dto.FromAccountId);
        if (account == null || account.UserId != userId) return false;

        if (account.Balance < dto.Amount)
            throw new ArgumentException("Hesap bakiyesi yetersiz.");

        // Hesaptan para düş
        var success = await _transactionService.MakeWithdrawAsync(account.Id, dto.Amount);
        if (!success) return false;

        // Limiti güncelle (Borçdan düş)
        card.UsedAmount -= dto.Amount;
        if (card.UsedAmount < 0) card.UsedAmount = 0; // Fazla ödenirse sıfırla

        await _creditCardRepository.UpdateAsync(card);

        var transaction = new CreditCardTransaction
        {
            CreditCardId = card.Id,
            Amount = -dto.Amount, // Eksi bakiye ödemeyi temsil eder
            Description = "Kart Borcu Ödemesi",
            Category = "Ödeme",
            CreatedAt = DateTime.UtcNow
        };
        await _creditCardRepository.AddTransactionAsync(transaction);

        return true;
    }

    public async Task<bool> CashAdvanceAsync(int userId, CashAdvanceDto dto)
    {
        var card = await _creditCardRepository.GetByIdAsync(dto.CreditCardId);
        if (card == null || card.UserId != userId || !card.IsActive) return false;

        // %5 Nakit avans faizi/komisyonu
        decimal totalDebt = dto.Amount * 1.05m;

        if (card.AvailableLimit < totalDebt)
            throw new ArgumentException("Nakit avans limiti (komisyon dahil) yetersiz.");

        // Hesaba tutarı yatır (deposist)
        var account = await _accountRepository.GetByIdAsync(dto.ToAccountId);
        if (account == null || account.UserId != userId) return false;

        var depositSuccess = await _transactionService.MakeDepositAsync(account.Id, dto.Amount);
        if (!depositSuccess) return false;

        // Kart borcuna ekle
        card.UsedAmount += totalDebt;
        await _creditCardRepository.UpdateAsync(card);

        var transaction = new CreditCardTransaction
        {
            CreditCardId = card.Id,
            Amount = totalDebt,
            Description = $"Nakit Avans ({dto.Installments} Taksit, %5 Komisyon dahil)",
            Category = "NakitAvans",
            CreatedAt = DateTime.UtcNow
        };
        await _creditCardRepository.AddTransactionAsync(transaction);

        return true;
    }

    public async Task<bool> UpdateLimitAsync(int userId, int cardId, decimal newLimit)
    {
        var card = await _creditCardRepository.GetByIdAsync(cardId);
        if (card == null || card.UserId != userId) return false;

        card.CardLimit = newLimit;
        await _creditCardRepository.UpdateAsync(card);
        return true;
    }

    private static CreditCardDto MapToDto(CreditCard card) => new()
    {
        Id = card.Id,
        CardNumber = card.CardNumber,
        Cvv = card.Cvv,
        ExpiryDate = card.ExpiryDate,
        CardLimit = card.CardLimit,
        UsedAmount = card.UsedAmount,
        AvailableLimit = card.AvailableLimit,
        MinPayment = card.MinPayment,
        IsActive = card.IsActive,
        Transactions = card.Transactions?.Select(t => new CreditCardTransactionDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Description = t.Description,
            Category = t.Category,
            CreatedAt = t.CreatedAt
        }).ToList() ?? new List<CreditCardTransactionDto>()
    };
}
