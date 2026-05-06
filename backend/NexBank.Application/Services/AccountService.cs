using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Application.Patterns.Factory;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountFactory _accountFactory; // ← FACTORY PATTERN kullanımı

    public AccountService(IAccountRepository accountRepository, IAccountFactory accountFactory)
    {
        _accountRepository = accountRepository;
        _accountFactory = accountFactory;
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<List<AccountDto>> GetAccountsByUserIdAsync(int userId)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(userId);
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<AccountDto?> GetAccountByIdAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        return account == null ? null : MapToDto(account);
    }

    /// <summary>
    /// FACTORY PATTERN burada kullanılıyor!
    /// Hesap tipine göre AccountFactory farklı özelliklerle Account üretiyor.
    /// </summary>
    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)
    {
        // String'i enum'a çevir
        if (!Enum.TryParse<AccountType>(dto.AccountType, true, out var accountType))
            throw new ArgumentException($"Geçersiz hesap tipi: {dto.AccountType}");

        // Factory Pattern ile hesap oluştur
        var account = _accountFactory.CreateAccount(accountType, dto.UserId, dto.Currency);

        // Veritabanına kaydet
        var saved = await _accountRepository.AddAsync(account);
        return MapToDto(saved);
    }

    public async Task<AccountDto?> UpgradeAccountAsync(int accountId, string newType)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null) return null;

        if (!Enum.TryParse<AccountType>(newType, true, out var parsedType))
            throw new ArgumentException("Geçersiz hesap tipi.");

        // Tip ve limit yükseltme mantığı (Business Rule)
        account.AccountType = parsedType;
        if (parsedType == AccountType.Corporate) account.DailyLimit = 100000;
        else if (parsedType == AccountType.Deposit) account.DailyLimit = 50000;

        await _accountRepository.UpdateAsync(account);
        return MapToDto(account);
    }

    private static AccountDto MapToDto(Account account) => new()
    {
        Id = account.Id,
        AccountNumber = account.AccountNumber,
        Iban = account.Iban,
        AccountType = account.AccountType.ToString(),
        Balance = account.Balance,
        Currency = account.Currency,
        DailyLimit = account.DailyLimit,
        IsActive = account.IsActive,
        CreatedAt = account.CreatedAt,
        UserId = account.UserId,
        UserFullName = account.User?.FullName ?? ""
    };
}
