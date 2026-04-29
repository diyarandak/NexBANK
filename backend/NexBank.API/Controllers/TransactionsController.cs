using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> MakeTransfer([FromBody] TransferRequestDto request)
    {
        try
        {
            var success = await _transactionService.MakeTransferAsync(
                request.FromAccountId, 
                request.ToIban, // IBAN İLE
                request.Amount, 
                request.PaymentMethod);

            if (success)
            {
                return Ok(new { Message = "Transfer başarıyla gerçekleştirildi." });
            }
            
            return BadRequest(new { Message = "Bakiye yetersiz veya geçersiz hesap." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu hatası: " + ex.Message });
        }
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositWithdrawDto request)
    {
        try
        {
            var success = await _transactionService.MakeDepositAsync(request.AccountId, request.Amount);
            if (success) return Ok(new { Message = "Para başarıyla yatırıldı." });
            return BadRequest(new { Message = "İşlem başarısız." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] DepositWithdrawDto request)
    {
        try
        {
            var success = await _transactionService.MakeWithdrawAsync(request.AccountId, request.Amount);
            if (success) return Ok(new { Message = "Para başarıyla çekildi." });
            return BadRequest(new { Message = "Bakiye yetersiz veya limit aşıldı." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("undo/{transactionId}")]
    public async Task<IActionResult> UndoTransfer(int transactionId)
    {
        var success = await _transactionService.UndoLastTransferAsync(transactionId);
        if (success)
        {
            return Ok(new { Message = "İşlem başarıyla geri alındı." });
        }
        return BadRequest(new { Message = "İşlem geri alınamadı veya bulunamadı." });
    }

    [HttpGet("user/my")]
    public async Task<IActionResult> GetMyTransactions()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var transactions = await _transactionService.GetUserTransactionsAsync(userId);
        return Ok(transactions);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await _transactionService.GetAllTransactionsAsync();
        return Ok(transactions);
    }
}
