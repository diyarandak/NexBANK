using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Tüm hesapları listele
    /// GET /api/accounts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (roleClaim == "Admin")
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        else
        {
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();
                
            var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
            return Ok(accounts);
        }
    }

    /// <summary>
    /// Hesap detayı
    /// GET /api/accounts/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    /// <summary>
    /// Kullanıcının hesapları
    /// GET /api/accounts/user/{userId}
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
        return Ok(accounts);
    }

    /// <summary>
    /// FACTORY PATTERN ile yeni hesap oluştur
    /// POST /api/accounts
    /// Body: { "accountType": "Individual", "userId": 2, "currency": "TRY" }
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        try
        {
            var account = await _accountService.CreateAccountAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/upgrade")]
    public async Task<IActionResult> UpgradeAccount(int id, [FromBody] string newType)
    {
        try
        {
            var upgraded = await _accountService.UpgradeAccountAsync(id, newType);
            if (upgraded == null) return NotFound();
            return Ok(new { Message = "Hesap başarıyla yükseltildi.", Account = upgraded });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
