using MediatR;
using NexBank.Application.DTOs;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Patterns.Mediator;

/// <summary>
/// MEDIATOR PATTERN (MediatR): İşlem talebi (Request) ile işleyicisi (Handler) arasındaki bağı koparır.
/// </summary>

// Request (Soru)
public record GetUserAccountsQuery(int UserId) : IRequest<List<AccountDto>>;

// Handler (Cevap veren/İşleyen)
public class GetUserAccountsHandler : IRequestHandler<GetUserAccountsQuery, List<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetUserAccountsHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<AccountDto>> Handle(GetUserAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(request.UserId);
        return accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            Iban = a.Iban,
            Balance = a.Balance,
            Currency = a.Currency,
            AccountType = a.AccountType.ToString(),
            DailyLimit = a.DailyLimit,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            UserId = a.UserId,
            UserFullName = a.User?.FullName ?? ""
        }).ToList();
    }
}
