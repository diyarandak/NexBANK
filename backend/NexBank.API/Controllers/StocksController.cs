using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;

    public StocksController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStocks()
    {
        // Simulate change on each request for demo purposes
        await _stockService.SimulateMarketAsync();
        return Ok(await _stockService.GetAllStocksAsync());
    }

    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _stockService.GetUserPortfolioAsync(userId));
    }

    public class TradeRequest
    {
        public int AccountId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] TradeRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _stockService.BuyStockAsync(userId, request.AccountId, request.Symbol, request.Quantity);
        if (success) return Ok(new { Message = "Hisse başarıyla alındı." });
        return BadRequest(new { Message = "Yetersiz bakiye veya geçersiz işlem." });
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] TradeRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _stockService.SellStockAsync(userId, request.AccountId, request.Symbol, request.Quantity);
        if (success) return Ok(new { Message = "Hisse başarıyla satıldı." });
        return BadRequest(new { Message = "Yetersiz lot miktarı veya geçersiz işlem." });
    }
}
