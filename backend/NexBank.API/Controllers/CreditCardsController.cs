using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using System.Text.Json;


namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditCardsController : ControllerBase
{
    private readonly ICreditCardService _creditCardService;

    public CreditCardsController(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCreditCards()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var cards = await _creditCardService.GetUserCreditCardsAsync(userId);
        return Ok(cards);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCreditCard([FromBody] dynamic data)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        decimal limit = 10000m;
        try { 
            var json = data.ToString();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("limit", out JsonElement limitProp))
                limit = limitProp.GetDecimal();
        } catch {}

        var card = await _creditCardService.CreateCreditCardAsync(userId, limit);
        return Ok(new { Message = "Kredi kartı başarıyla oluşturuldu.", Card = card });
    }

    [HttpPost("spend")]
    public async Task<IActionResult> Spend([FromBody] CreditCardSpendDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var success = await _creditCardService.SpendAsync(userId, dto);
            if (success) return Ok(new { Message = "Harcama başarılı." });
            return BadRequest(new { Message = "İşlem başarısız." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("pay")]
    public async Task<IActionResult> PayDebt([FromBody] CreditCardPaymentDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var success = await _creditCardService.PayDebtAsync(userId, dto);
            if (success) return Ok(new { Message = "Borç ödemesi başarılı." });
            return BadRequest(new { Message = "İşlem başarısız. Lütfen bakiye ve bilgileri kontrol edin." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("cash-advance")]
    public async Task<IActionResult> CashAdvance([FromBody] CashAdvanceDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var success = await _creditCardService.CashAdvanceAsync(userId, dto);
            if (success) return Ok(new { Message = "Nakit avans başarıyla çekildi. %5 komisyon bakiyeye yansıtıldı." });
            return BadRequest(new { Message = "Limit yetersiz veya işlem başarısız." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/limit")]
    public async Task<IActionResult> UpdateLimit(int id, [FromBody] dynamic data)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        decimal newLimit = 0m;
        try {
            var json = data.ToString();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("limit", out JsonElement limitProp))
                newLimit = limitProp.GetDecimal();
        } catch {}

        if (newLimit <= 0) return BadRequest(new { Message = "Geçersiz limit değeri." });

        var success = await _creditCardService.UpdateLimitAsync(userId, id, newLimit);
        if (success) return Ok(new { Message = "Limit başarıyla güncellendi." });
        return BadRequest(new { Message = "İşlem başarısız." });
    }

    [HttpPost("virtual")]
    public async Task<IActionResult> CreateVirtualCard([FromBody] dynamic data)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        decimal limit = 1000m;
        try { 
            var json = data.ToString();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("limit", out JsonElement limitProp))
                limit = limitProp.GetDecimal();
        } catch {}

        var card = await _creditCardService.CreateCreditCardAsync(userId, limit);
        // İsteğe bağlı: card.IsVirtual = true; (Entity'de varsa)
        
        return Ok(new { Message = "Sanal kart başarıyla oluşturuldu.", Card = card });
    }
}
