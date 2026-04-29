using Microsoft.AspNetCore.SignalR;

namespace NexBank.API.Hubs;

/// <summary>
/// SIGNALR HUB: Client'lar (Frontend) ile sunucu arasında çift yönlü asenkron iletişim sağlar.
/// Borsa fiyatlarını anlık basmak için kullanılır.
/// </summary>
public class MarketHub : Hub
{
    // Client'ların dahil olabileceği gruplar (opsiyonel)
    public async Task JoinMarket()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "MarketWatchers");
    }
}
