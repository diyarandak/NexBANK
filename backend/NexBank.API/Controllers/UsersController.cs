using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Core.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoanRepository _loanRepository;

    public UsersController(IUserRepository userRepository, IAccountRepository accountRepository, ILoanRepository loanRepository)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _loanRepository = loanRepository;
    }

    /// <summary>
    /// Admin için müşteri listesi
    /// GET /api/users/customers
    /// </summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if ((roleClaim != "Admin" && roleClaim != "Staff") || userIdClaim == null || !int.TryParse(userIdClaim, out var staffId))
            return Forbid();

        // Personelin kendi şubesini bul
        var staff = await _userRepository.GetByIdAsync(staffId);
        if (staff == null) return Unauthorized();

        var users = await _userRepository.GetAllAsync();
        var customers = users
            .Where(u => u.Role == NexBank.Core.Entities.UserRole.Customer && 
                       (roleClaim == "Admin" || 
                        (!string.IsNullOrEmpty(u.Branch) && !string.IsNullOrEmpty(staff.Branch) && 
                         (u.Branch.Contains(staff.Branch.Replace(" Şubesi", ""), StringComparison.OrdinalIgnoreCase) || 
                          staff.Branch.Contains(u.Branch.Replace(" Şubesi", ""), StringComparison.OrdinalIgnoreCase)))))
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                TcKimlikNo = u.TcKimlikNo,
                Role = u.Role.ToString(),
                Branch = u.Branch,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            })
            .ToList();

        return Ok(customers);
    }
    /// <summary>
    /// Personel için müşterinin tüm finansal verilerini içeren detaylı görünüm
    /// </summary>
    [HttpGet("customer-360/{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetCustomer360(int id)
    {
        var staffBranch = User.FindFirst("Branch")?.Value;
        
        var u = await _userRepository.GetByIdAsync(id);
        if (u == null) return NotFound();
        if (u.Branch != staffBranch) return Forbid("Sadece kendi şubenizdeki müşterileri görebilirsiniz.");

        // Detaylı profil oluştur
        var profile = new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.TcKimlikNo,
            u.Branch,
            u.CreatedAt,
            Accounts = u.Accounts.Select(a => new { a.Iban, a.Balance, a.Currency, a.AccountType }),
            // Burada context üzerinden diğer veriler de çekilebilir (Loans, CreditCards vb.)
        };

        return Ok(profile);
    }

    [HttpPost("seed-mock-data")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedMockData()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var staffId)) return Unauthorized();

        var staff = await _userRepository.GetByIdAsync(staffId);
        if (staff == null) return Unauthorized();

        var names = new[] { "Ahmet Yılmaz", "Elif Demir", "Mehmet Kaya", "Selin Arslan", "Caner Yıldız", "Buse Çelik" };
        var tckns = new[] { "11111111111", "22222222222", "33333333333", "44444444444", "55555555555", "66666666666" };

        for (int i = 0; i < names.Length; i++)
        {
            var user = new NexBank.Core.Entities.User
            {
                FullName = names[i],
                Email = $"user{i}@nexbank.com",
                TcKimlikNo = tckns[i],
                PasswordHash = NexBank.Application.Services.AuthService.HashPassword("123Şifre321"),
                Role = NexBank.Core.Entities.UserRole.Customer,
                Branch = staff.Branch,
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            await _userRepository.AddAsync(user);

            // 1. Hesap Oluştur
            var account = new NexBank.Core.Entities.Account {
                UserId = user.Id,
                AccountNumber = $"ACC-{1000 + user.Id}",
                Iban = $"TR98000000000000000000{100 + user.Id}",
                Balance = 15000 + (i * 5000),
                Currency = "TRY",
                AccountType = NexBank.Core.Entities.AccountType.Individual,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _accountRepository.AddAsync(account);

            // 2. Kredi Başvurusu (Bazılarına bekleyen kredi ekle)
            if (i % 2 == 1)
            {
                var loan = new NexBank.Core.Entities.Loan {
                    UserId = user.Id,
                    AccountId = account.Id,
                    Amount = 50000 + (i * 10000),
                    LoanType = NexBank.Core.Entities.LoanType.Ihtiyac,
                    InterestRate = 3.45m,
                    TermMonths = 24,
                    MonthlyPayment = 3500,
                    TotalPayment = 84000,
                    RemainingBalance = 84000,
                    Status = NexBank.Core.Entities.LoanStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _loanRepository.AddAsync(loan);
            }
        }

        return Ok(new { Message = "Sisteme 6 adet örnek müşteri, hesap ve kredi başvurusu eklendi." });
    }
}
