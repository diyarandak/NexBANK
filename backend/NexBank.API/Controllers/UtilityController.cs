using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilityController : ControllerBase
{
    private readonly IBillService _billService;
    private readonly IFavoriteRecipientService _favoriteService;
    private readonly IStandingOrderService _standingOrderService;

    public UtilityController(IBillService billService, IFavoriteRecipientService favoriteService, IStandingOrderService standingOrderService)
    {
        _billService = billService;
        _favoriteService = favoriteService;
        _standingOrderService = standingOrderService;
    }

    private int GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out int userId) ? userId : 0;
    }

    // ── FATURA (BILLS) ENDPOINTS ──
    [HttpGet("bills")]
    public async Task<IActionResult> GetBills()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        return Ok(await _billService.GetUserBillsAsync(userId));
    }

    [HttpPost("bills/mock")]
    public async Task<IActionResult> MockBill([FromBody] CreateBillDto dto)
    {
        // Kullanıcı kendi faturasını sistemde üretip sümule edebilecek
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        var bill = await _billService.CreateBillAsync(userId, dto);
        return Ok(new { Message = "Sanal fatura oluşturuldu.", Bill = bill });
    }

    [HttpPost("bills/pay")]
    public async Task<IActionResult> PayBill([FromBody] PayBillDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        
        var success = await _billService.PayBillAsync(userId, dto);
        if (success) return Ok(new { Message = "Fatura başarıyla ödendi." });
        return BadRequest(new { Message = "Fatura ödemesi başarısız. (Yetersiz bakiye veya zaten ödenmiş)" });
    }

    // ── FAVORITES ENDPOINTS ──
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        return Ok(await _favoriteService.GetUserFavoritesAsync(userId));
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> AddFavorite([FromBody] CreateFavoriteDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        var fav = await _favoriteService.AddFavoriteAsync(userId, dto);
        return Ok(new { Message = "Favori başarıyla eklendi.", Favorite = fav });
    }

    [HttpDelete("favorites/{id}")]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        var success = await _favoriteService.RemoveFavoriteAsync(id, userId);
        if (success) return Ok(new { Message = "Favori silindi." });
        return BadRequest(new { Message = "Favori silinemedi." });
    }

    // ── STANDING ORDERS (OTOMATİK ÖDEME) ENDPOINTS ──
    [HttpGet("standing-orders")]
    public async Task<IActionResult> GetStandingOrders()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        return Ok(await _standingOrderService.GetUserStandingOrdersAsync(userId));
    }

    [HttpPost("standing-orders")]
    public async Task<IActionResult> AddStandingOrder([FromBody] CreateStandingOrderDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();
            var order = await _standingOrderService.AddStandingOrderAsync(userId, dto);
            return Ok(new { Message = "Otomatik ödeme talimatı oluşturuldu.", Order = order });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("standing-orders/{id}")]
    public async Task<IActionResult> RemoveStandingOrder(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        var success = await _standingOrderService.RemoveStandingOrderAsync(id, userId);
        if (success) return Ok(new { Message = "Talimat iptal edildi." });
        return BadRequest(new { Message = "Talimat iptal edilemedi." });
    }

    // Cronjob simülatör
    [HttpPost("standing-orders/process")]
    public async Task<IActionResult> ProcessOrders()
    {
        await _standingOrderService.ProcessPendingOrdersAsync(DateTime.UtcNow);
        return Ok(new { Message = "Günü gelen otomatik ödemeler çalıştırıldı." });
    }
}
