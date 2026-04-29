using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Observer;

public class SmsNotificationObserver : ITransactionObserver
{
    public async Task UpdateAsync(Transaction transaction)
    {
        // Gerçekte bir SMS servisi (Twilio vb.) çağırılır
        Console.WriteLine($"[SMS ONSERVER]: Gönderen {transaction.FromAccountId} hesabına {transaction.Amount} {transaction.Type} işlemi için SMS gönderildi.");
        await Task.CompletedTask;
    }
}
