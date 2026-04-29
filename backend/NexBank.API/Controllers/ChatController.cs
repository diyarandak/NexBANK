using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexBank.Infrastructure.Persistence;
using System.Security.Claims;
using NexBank.Core.Entities;


namespace NexBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly NexBankDbContext _context;

    public ChatController(NexBankDbContext context) => _context = context;

    /// <summary>
    /// Mesaj geçmişini getir
    /// </summary>
    [HttpGet("history/{otherUserId}")]
    public async Task<IActionResult> GetChatHistory(int otherUserId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        
        List<ChatMessage> messages;
        
        if (role == "Customer")
        {
            messages = await _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
        else
        {
            messages = await _context.ChatMessages
                .Where(m => m.SenderId == otherUserId || m.ReceiverId == otherUserId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        return Ok(messages);
    }

    /// <summary>
    /// Yeni mesaj gönder
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var branch = User.FindFirst("Branch")?.Value ?? "";

        var chatMsg = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId > 0 ? dto.ReceiverId : null,
            Content = dto.Message,
            Branch = branch,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.ChatMessages.Add(chatMsg);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, messageId = chatMsg.Id, sentAt = chatMsg.SentAt });
    }

    /// <summary>
    /// Okunmamış mesaj sayısını getir
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        int count;
        if (role == "Customer")
        {
            // Müşteri: Kendisine gelen okunmamış mesajlar
            count = await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);
        }
        else
        {
            // Personel: Tüm okunmamış müşteri mesajları
            count = await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == null && !m.IsRead);
        }

        return Ok(new { unreadCount = count });
    }

    /// <summary>
    /// Mesajları okundu olarak işaretle
    /// </summary>
    [HttpPost("mark-read/{otherUserId}")]
    public async Task<IActionResult> MarkAsRead(int otherUserId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        List<ChatMessage> unread;
        if (role == "Customer")
        {
            // Müşteri: Personelden gelen mesajları okundu yap
            unread = await _context.ChatMessages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead && m.SenderId != currentUserId)
                .ToListAsync();
        }
        else
        {
            // Personel: Bu müşteriden gelen mesajları okundu yap
            unread = await _context.ChatMessages
                .Where(m => m.SenderId == otherUserId && !m.IsRead)
                .ToListAsync();
        }

        foreach (var msg in unread)
        {
            msg.IsRead = true;
        }
        await _context.SaveChangesAsync();

        return Ok(new { markedCount = unread.Count });
    }
}

public class SendMessageDto
{
    public int ReceiverId { get; set; }
    public string Message { get; set; } = "";
}
