using Microsoft.AspNetCore.SignalR;
using NexBank.API.Hubs;
using NexBank.Application.Interfaces;

namespace NexBank.API.Services;

/// <summary>
/// BACKGROUND SERVICE: Borsa fiyatlarını arka planda sürekli günceller.
/// Her güncellemede SIGNALR üzerinden bağlı olan tüm client'lara yeni fiyatları basar.
/// </summary>
public class MarketUpdateWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<MarketHub> _hubContext;

    public MarketUpdateWorker(IServiceProvider serviceProvider, IHubContext<MarketHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // İlk başlatmada kısa bir bekleme
            await Task.Delay(3000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                    await stockService.SimulateMarketAsync();
                    var stocks = await stockService.GetAllStocksAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveStockUpdates", stocks, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"[MarketWorker] Hata: {ex.Message}");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Uygulama kapanıyor — normal
        }
    }
}
