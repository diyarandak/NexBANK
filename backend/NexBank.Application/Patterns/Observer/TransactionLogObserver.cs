using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Observer;

public class TransactionLogObserver : ITransactionObserver
{
    public async Task UpdateAsync(Transaction transaction)
    {
        // Log mekanizması (Elasticsearch, File, vs.)
        Console.WriteLine($"[LOG OBSERVER]: İşlem {transaction.Id} başarıyla Audit günlüğüne kaydedildi.");
        await Task.CompletedTask;
    }
}
