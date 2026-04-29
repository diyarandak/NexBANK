using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BankSettingsController : ControllerBase
{
    // Gerçekte bu veriler DB'den gelir, şimdilik statik bir yönetici hafızası kuralım
    private static decimal _loanRate = 3.50m;
    private static decimal _transferFee = 1.00m;
    private static decimal _savingRate = 45.0m;

    [HttpGet]
    public IActionResult GetSettings()
    {
        return Ok(new { LoanRate = _loanRate, TransferFee = _transferFee, SavingRate = _savingRate });
    }

    public class UpdateSettingsDto { public decimal LoanRate { get; set; } public decimal TransferFee { get; set; } }

    [HttpPost("update")]
    public IActionResult UpdateSettings([FromBody] UpdateSettingsDto request)
    {
        _loanRate = request.LoanRate;
        _transferFee = request.TransferFee;
        return Ok(new { Message = "Banka parametreleri başarıyla güncellendi. Tüm şubelerde geçerli hale getirildi." });
    }

    // Müşteri tarafının güncel oranları alması için (Authorize rollerini esnetebiliriz)
    [HttpGet("public-rates")]
    [AllowAnonymous]
    public IActionResult GetPublicRates()
    {
        return Ok(new { LoanRate = _loanRate, TransferFee = _transferFee });
    }
}
