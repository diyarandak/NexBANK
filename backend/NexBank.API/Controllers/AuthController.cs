using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Müşteri Girişi — TC Kimlik No + Şifre
    /// POST /api/auth/customer-login
    /// </summary>
    [HttpPost("customer-login")]
    public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginRequestDto request)
    {
        var result = await _authService.CustomerLoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Geçersiz TC Kimlik No veya şifre. Kayıtlı değilseniz lütfen önce kayıt olunuz." });

        return Ok(result);
    }

    /// <summary>
    /// Personel Girişi — E-posta + Şifre
    /// POST /api/auth/staff-login
    /// </summary>
    [HttpPost("staff-login")]
    public async Task<IActionResult> StaffLogin([FromBody] StaffLoginRequestDto request)
    {
        var result = await _authService.StaffLoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Geçersiz e-posta veya şifre. Kayıtlı bir personel hesabınız yoksa lütfen yöneticinize başvurunuz." });

        return Ok(result);
    }

    /// <summary>
    /// Müşteri Kaydı — TC, Ad Soyad, E-posta, Şifre
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterCustomerAsync(request);
            return Ok(new { message = "Kayıt başarıyla tamamlandı! Artık TC Kimlik No ve şifrenizle giriş yapabilirsiniz.", user });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Personel Kaydı
    /// POST /api/auth/register-staff
    /// </summary>
    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterStaffAsync(request);
            return Ok(new { message = "Personel hesabı başarıyla oluşturuldu.", user });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut kullanıcı bilgisi
    /// GET /api/auth/me
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null) return NotFound();

        return Ok(user);
    }
}
