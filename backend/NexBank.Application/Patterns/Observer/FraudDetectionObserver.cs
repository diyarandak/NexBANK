using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Patterns.Observer;

public class FraudDetectionObserver : ITransactionObserver
{
    private readonly decimal FRAUD_LIMIT = 50000m; // 50 Bin TL üstü şüpheli

    public Task UpdateAsync(Transaction transaction)
    {
        // Sadece transferleri ve yüksek tutarları incele
        if (transaction.Type == TransactionType.Transfer && transaction.Amount >= FRAUD_LIMIT)
        {
            Console.WriteLine($"[FRAUD ALERT] 🚨 Şüpheli İşlem Yakalandı! İşlem ID: {transaction.Id}, Tutar: {transaction.Amount} ₺");
            
            // Burada normalde Fraud Alert tablosuna kayıt atılır veya SignalR ile Personele canlı bildirim gider.
            // Şimdilik işlemi durdurmuyoruz ama "İncelenmesi Gerekenler" havuzuna düştüğünü varsayıyoruz.
            transaction.Status = TransactionStatus.Pending; // İşlemi beklemeye al (Opsiyonel)
            transaction.Description += " [🚨 ŞÜPHELİ İŞLEM - İNCELEMEDE]";
        }

        return Task.CompletedTask;
    }
}
