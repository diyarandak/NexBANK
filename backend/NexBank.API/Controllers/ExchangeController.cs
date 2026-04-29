using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpGet("rates")]
    public async Task<IActionResult> GetRates()
    {
        return Ok(await _exchangeService.GetExchangeRatesAsync());
    }

    public class ExchangeRequestDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal AmountToExchange { get; set; }
        public string TargetCurrency { get; set; } = string.Empty;
    }

    [HttpPost("swap")]
    public async Task<IActionResult> SwapCurrency([FromBody] ExchangeRequestDto request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var success = await _exchangeService.ExchangeCurrencyAsync(userId, request.FromAccountId, request.ToAccountId, request.AmountToExchange, request.TargetCurrency);
        if (success) return Ok(new { Message = "Döviz alımı/satımı başarıyla gerçekleşti." });
        return BadRequest(new { Message = "Döviz işlemi başarısız. Lütfen bakiyenizi ve seçili hedef para birimini kontrol edin." });
    }
}
