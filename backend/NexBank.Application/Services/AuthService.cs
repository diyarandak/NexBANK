using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly NexBank.Application.Patterns.Factory.IAccountFactory _accountFactory;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository, 
        IConfiguration configuration,
        NexBank.Application.Patterns.Factory.IAccountFactory accountFactory,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _accountFactory = accountFactory;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Müşteri Girişi — TC Kimlik No + Şifre
    /// </summary>
    public async Task<LoginResponseDto?> CustomerLoginAsync(CustomerLoginRequestDto request)
    {
        var user = await _userRepository.GetByTcKimlikAsync(request.TcKimlikNo);
        if (user == null || user.Role != Core.Entities.UserRole.Customer)
            return null;

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        return new LoginResponseDto { Token = token, User = MapToDto(user) };
    }

    /// <summary>
    /// Personel Girişi — E-posta + Şifre
    /// </summary>
    public async Task<LoginResponseDto?> StaffLoginAsync(StaffLoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || user.Role != Core.Entities.UserRole.Admin)
            return null;

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        return new LoginResponseDto { Token = token, User = MapToDto(user) };
    }

    /// <summary>
    /// Müşteri Kaydı — TC, Ad Soyad, E-posta, Şifre
    /// Kayıt sonrası Factory Pattern ile otomatik hesap açılır
    /// </summary>
    public async Task<UserDto> RegisterCustomerAsync(RegisterRequestDto request)
    {
        // 1. TC Kimlik No Doğrulama (11 haneli sayı)
        if (string.IsNullOrWhiteSpace(request.TcKimlikNo) || request.TcKimlikNo.Length != 11 || !request.TcKimlikNo.All(char.IsDigit))
        {
            throw new ArgumentException("TC Kimlik No 11 haneli rakamlardan oluşmalıdır.");
        }

        // 2. Şifre Güvenlik Kontrolü
        if (request.Password.Length < 8 || !request.Password.Any(char.IsDigit) || !request.Password.Any(char.IsUpper))
        {
            throw new ArgumentException("Şifre en az 8 karakter olmalı, en az bir sayı ve bir büyük harf içermelidir.");
        }

        // 3. TC benzersizliği
        var existingTc = await _userRepository.GetByTcKimlikAsync(request.TcKimlikNo);
        if (existingTc != null)
        {
            throw new ArgumentException("Bu TC Kimlik No ile zaten bir hesap bulunmaktadır.");
        }

        // 4. Email benzersizliği
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ArgumentException("Bu e-posta adresi sistemde zaten kayıtlı.");
        }

        // 5. Transaction Başlat
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 6. Kullanıcıyı Kaydet
            var userBranch = (request.Branch != null && request.Branch.Contains("Yalova")) ? "Yalova Şubesi" : (request.Branch ?? "Genel");

            var user = new Core.Entities.User
            {
                FullName = request.FullName,
                Email = request.Email,
                TcKimlikNo = request.TcKimlikNo,
                PasswordHash = HashPassword(request.Password),
                Role = Core.Entities.UserRole.Customer,
                Branch = userBranch,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var savedUser = await _userRepository.AddAsync(user);
            
            if (savedUser == null || savedUser.Id == 0)
            {
                throw new Exception("Kullanıcı kaydı yapılamadı.");
            }

            // 7. Varsayılan bir hesap aç (Factory Pattern)
            var defaultAccount = _accountFactory.CreateAccount(Core.Entities.AccountType.Individual, savedUser.Id, "TRY");
            await _accountRepository.AddAsync(defaultAccount);

            await _unitOfWork.CommitTransactionAsync();
            return MapToDto(savedUser);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Personel Kaydı — Ad Soyad, E-posta, Şifre
    /// </summary>
    public async Task<UserDto> RegisterStaffAsync(StaffRegisterRequestDto request)
    {
        if (request.Password.Length < 8 || !request.Password.Any(char.IsDigit) || !request.Password.Any(char.IsUpper))
        {
            throw new ArgumentException("Şifre en az 8 karakter olmalı, en az bir sayı ve bir büyük harf içermelidir.");
        }

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ArgumentException("Bu e-posta adresi sistemde zaten kayıtlı.");
        }

        var user = new Core.Entities.User
        {
            FullName = request.FullName,
            Email = request.Email,
            TcKimlikNo = "", // Personel TC ile girmez
            PasswordHash = HashPassword(request.Password),
            Role = Core.Entities.UserRole.Admin,
            Branch = request.Branch ?? "Merkez Şube", // Şube ataması eklendi
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var savedUser = await _userRepository.AddAsync(user);
        return MapToDto(savedUser);
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;
        return MapToDto(user);
    }

    // ── Yardımcı Metotlar ──

    private UserDto MapToDto(Core.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            TcKimlikNo = user.TcKimlikNo,
            Role = user.Role.ToString(),
            Branch = user.Branch,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }

    private string GenerateJwtToken(Core.Entities.User user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "NexBankSuperSecretKey2024!@#$%^&*()VeryLong";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Branch", user.Branch ?? "Genel") // Şube bilgisini token'a ekle
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "NexBank",
            audience: _configuration["Jwt:Audience"] ?? "NexBankApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Basit SHA256 hash — production'da BCrypt kullanılır
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
