using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Observer;

public class EmailNotificationObserver : ITransactionObserver
{
    public async Task UpdateAsync(Transaction transaction)
    {
        // Alıcı ve göndericiye email atma simülasyonu
        Console.WriteLine($"[EMAIL OBSERVER]: İşlem #{transaction.Id} detayları e-posta ile iletildi.");
        await Task.CompletedTask;
    }
}
