using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserLoans()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var loans = await _loanService.GetUserLoansAsync(userId);
        return Ok(loans);
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var loan = await _loanService.ApplyForLoanAsync(userId, dto);
            return Ok(new { Message = "Kredi başvurusu başarıyla alındı. Onay bekleniyor.", Loan = loan });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("pay/{paymentId}")]
    public async Task<IActionResult> PayInstallment(int paymentId, [FromBody] PayBillDto request)
    {
        var success = await _loanService.PayInstallmentAsync(paymentId, request.AccountId);
        if (success)
            return Ok(new { Message = "Taksit başarıyla ödendi." });

        return BadRequest(new { Message = "Bakiye yetersiz veya taksit bulunamadı/zaten ödenmiş." });
    }

    // ── ADMIN ENDPOINTS ──

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingLoans()
    {
        var loans = await _loanService.GetPendingLoansAsync();
        return Ok(loans);
    }

    [HttpPost("{loanId}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveLoan(int loanId)
    {
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(staffIdStr, out int staffId)) return Unauthorized();

        var success = await _loanService.ApproveLoanAsync(loanId, staffId);
        if (success) return Ok(new { Message = "Kredi onaylandı ve tutar hesaba yatırıldı." });

        return BadRequest(new { Message = "Kredi onaylanamadı. (Zaten onaylı veya bulunamadı)" });
    }

    [HttpPost("{loanId}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectLoan(int loanId)
    {
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(staffIdStr, out int staffId)) return Unauthorized();

        var success = await _loanService.RejectLoanAsync(loanId, staffId);
        if (success) return Ok(new { Message = "Kredi reddedildi." });

        return BadRequest(new { Message = "Kredi reddedilemedi." });
    }
}
