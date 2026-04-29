using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NexBank.Infrastructure.Persistence;
using NexBank.Core.Entities;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace NexBank.API.Hubs;

/// <summary>
/// Canlı Destek Hub'ı — Observer Pattern (SignalR) ile gerçek zamanlı mesajlaşma.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly ConcurrentDictionary<int, string> _userConnections = new();

    public ChatHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private int GetUserId()
    {
        // NameIdentifier claim'ini bul
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            // Fallback: "sub" claim'ini dene
            claim = Context.User?.FindFirst("sub");
        }
        if (claim == null)
        {
            // Tüm claim'leri logla
            var allClaims = Context.User?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
            Console.WriteLine($"[ChatHub] UYARI: NameIdentifier bulunamadı. Claims: {string.Join(", ", allClaims)}");
            return 0;
        }
        return int.TryParse(claim.Value, out int id) ? id : 0;
    }

    private string GetUserName()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst("name")?.Value
            ?? Context.User?.Identity?.Name 
            ?? "Kullanıcı";
    }

    private string GetUserBranch()
    {
        return Context.User?.FindFirst("Branch")?.Value ?? "";
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var branch = GetUserBranch();
        var name = GetUserName();

        Console.WriteLine($"[ChatHub] BAĞLANDI: UserId={userId}, Name={name}, Branch={branch}, ConnId={Context.ConnectionId}");

        if (userId > 0)
        {
            _userConnections[userId] = Context.ConnectionId;
        }

        if (!string.IsNullOrEmpty(branch))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, branch);
            Console.WriteLine($"[ChatHub] Gruba eklendi: {branch}");
        }
        else
        {
            Console.WriteLine($"[ChatHub] UYARI: Branch boş! Kullanıcı hiçbir gruba eklenmedi.");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        Console.WriteLine($"[ChatHub] AYRILDI: UserId={userId}, Hata={exception?.Message ?? "yok"}");
        
        if (userId > 0)
        {
            _userConnections.TryRemove(userId, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Müşteri → Şube Personeline mesaj gönderir
    /// </summary>
    public async Task SendMessageToBranchStaff(string message)
    {
        var senderId = GetUserId();
        var senderName = GetUserName();
        var branch = GetUserBranch();

        Console.WriteLine($"[ChatHub] SendMessageToBranchStaff: senderId={senderId}, name={senderName}, branch='{branch}', msg='{message}'");

        if (senderId == 0)
        {
            Console.WriteLine("[ChatHub] HATA: senderId == 0, mesaj gönderilemez!");
            return;
        }

        if (string.IsNullOrEmpty(branch))
        {
            Console.WriteLine("[ChatHub] UYARI: Branch boş, grup mesajı gönderilemez! Caller'a geri gönderiliyor.");
            // Branch yoksa sadece caller'a bildir
            await Clients.Caller.SendAsync("ReceiveSupportMessage", senderId, senderName, message);
            return;
        }

        // DB kayıt (arka planda)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NexBankDbContext>();
                db.ChatMessages.Add(new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = null,
                    Content = message,
                    Branch = branch,
                    SentAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                Console.WriteLine($"[ChatHub] Mesaj DB'ye kaydedildi (senderId={senderId})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] DB kayıt hatası: {ex.Message}");
            }
        });

        // Mesajı şube grubuna gönder
        Console.WriteLine($"[ChatHub] Grup mesajı gönderiliyor: branch='{branch}'");
        await Clients.Group(branch).SendAsync("ReceiveSupportMessage", senderId, senderName, message);
        Console.WriteLine($"[ChatHub] Mesaj başarıyla gönderildi!");
    }

    /// <summary>
    /// Personel → Belirli müşteriye cevap verir
    /// </summary>
    public async Task ReplyToCustomer(int customerId, string message)
    {
        var senderId = GetUserId();
        var senderName = GetUserName();
        var branch = GetUserBranch();

        Console.WriteLine($"[ChatHub] ReplyToCustomer: senderId={senderId}, customerId={customerId}, msg='{message}'");

        if (senderId == 0) return;

        // DB kayıt (arka planda)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NexBankDbContext>();
                db.ChatMessages.Add(new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = customerId,
                    Content = message,
                    Branch = branch,
                    SentAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] DB kayıt hatası: {ex.Message}");
            }
        });

        // Müşteriye doğrudan gönder
        if (_userConnections.TryGetValue(customerId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveSupportReply", senderId, senderName, message);
            Console.WriteLine($"[ChatHub] Müşteriye doğrudan gönderildi (connId={connectionId})");
        }
        else
        {
            Console.WriteLine($"[ChatHub] Müşteri çevrimdışı (customerId={customerId})");
        }

        // Şube grubuna bildir
        if (!string.IsNullOrEmpty(branch))
        {
            await Clients.Group(branch).SendAsync("ReceiveSupportMessage", senderId, senderName, message);
        }
    }
}
