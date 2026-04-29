using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public NotificationsController(INotificationRepository notificationRepository, IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        
        var notifications = await _notificationRepository.GetByUserIdAsync(int.Parse(userIdStr));
        return Ok(notifications);
    }

    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Broadcast([FromBody] string message)
    {
        var customers = await _userRepository.GetAllAsync(); // Gerçekte sadece Customerlar seçilir
        foreach (var customer in customers)
        {
            var notification = new Notification
            {
                UserId = customer.Id,
                Title = "Banka Duyurusu",
                Message = message,
                Type = "Info",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.AddAsync(notification);
        }
        return Ok(new { Message = "Duyuru tüm müşterilere başarıyla iletildi." });
    }
}
