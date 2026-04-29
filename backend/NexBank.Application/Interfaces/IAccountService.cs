using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IAccountService
{
    Task<List<AccountDto>> GetAllAccountsAsync();
    Task<List<AccountDto>> GetAccountsByUserIdAsync(int userId);
    Task<AccountDto?> GetAccountByIdAsync(int id);
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto);
    Task<AccountDto?> UpgradeAccountAsync(int accountId, string newType);
}
