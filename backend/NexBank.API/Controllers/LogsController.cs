using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAuditLogs()
    {
        // Gerçek bir sistemde bu veriler DB'den gelir. 
        // Burada personelin "Vay canına!" demesi için simüle edilmiş gerçekçi veriler dönüyoruz.
        var logs = new List<object>
        {
            new { Id = 1, Action = "Sistem Girişi", User = "Diyar Andak", Detail = "Yalova Merkez Şubesi aktif edildi.", Severity = "Info", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
            new { Id = 2, Action = "Kredi Onayı", User = "Ahmet Kılıç", Detail = "75.000 ₺ tutarında konut kredisi onaylandı.", Severity = "Success", Timestamp = DateTime.UtcNow.AddMinutes(-12) },
            new { Id = 3, Action = "Riskli İşlem", User = "Sistem", Detail = "Müşteri #124 için ardışık 3 başarısız giriş denemesi.", Severity = "Warning", Timestamp = DateTime.UtcNow.AddMinutes(-45) },
            new { Id = 4, Action = "Hesap Dondurma", User = "Fatma Yıldırım", Detail = "Hatalı TCKN eşleşmesi nedeniyle hesap geçici olarak askıya alındı.", Severity = "Danger", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new { Id = 5, Action = "Yüksek Transfer", User = "Sistem", Detail = "1.200.000 ₺ tutarında yurt dışı transferi tespit edildi.", Severity = "Warning", Timestamp = DateTime.UtcNow.AddHours(-3) },
            new { Id = 6, Action = "Yeni Personel Kaydı", User = "Diyar Andak", Detail = "Selin Yılmaz 'Müşteri Temsilcisi' olarak atandı.", Severity = "Success", Timestamp = DateTime.UtcNow.AddHours(-5) }
        };

        return Ok(logs);
    }
}
