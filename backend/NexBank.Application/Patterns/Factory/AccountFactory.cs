using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Factory;

/// <summary>
/// FACTORY PATTERN — Concrete Implementation
/// 
/// Hesap tipine göre farklı özelliklerle Account nesnesi üretir.
/// - Individual (Bireysel): 50.000₺ günlük limit
/// - Corporate (Kurumsal): 500.000₺ günlük limit
/// - Savings (Tasarruf): 10.000₺ günlük limit, faiz aktif
/// - Deposit (Vadeli): 0₺ günlük limit (çekim yok), vade bazlı
/// 
/// Bu pattern sayesinde yeni hesap tipleri eklemek kolay:
/// sadece yeni bir case eklemek yeterli.
/// </summary>
public class AccountFactory : IAccountFactory
{
    public Account CreateAccount(AccountType type, int userId, string currency = "TRY")
    {
        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(type),
            Iban = GenerateIban(),
            AccountType = type,
            Balance = 0,
            Currency = currency,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Her hesap tipine göre farklı kurallar uygulanır
        switch (type)
        {
            case AccountType.Individual:
                account.DailyLimit = 50_000m;
                break;

            case AccountType.Corporate:
                account.DailyLimit = 500_000m;
                break;

            case AccountType.Savings:
                account.DailyLimit = 10_000m;
                break;

            case AccountType.Deposit:
                account.DailyLimit = 0m; // Vadeli hesaptan günlük çekim yok
                break;

            default:
                throw new ArgumentException($"Bilinmeyen hesap tipi: {type}");
        }

        return account;
    }

    /// <summary>
    /// Hesap numarası üretir. Tip koduna göre prefix eklenir.
    /// NB = NexBank, sonra tip kodu, sonra rastgele 8 hane.
    /// </summary>
    private static string GenerateAccountNumber(AccountType type)
    {
        var prefix = type switch
        {
            AccountType.Individual => "NB-BIR",
            AccountType.Corporate => "NB-KUR",
            AccountType.Savings   => "NB-TAS",
            AccountType.Deposit   => "NB-VAD",
            _ => "NB-XXX"
        };

        var random = new Random();
        var number = random.Next(10000000, 99999999);
        return $"{prefix}-{number}";
    }

    /// <summary>
    /// TR IBAN üretir: TR + 2 kontrol + 4 banka kodu + 16 hesap numarası = 26 karakter
    /// </summary>
    private static string GenerateIban()
    {
        var random = new Random();
        var kontrolHanesi = random.Next(10, 99);
        var hesapNo = "";
        for (int i = 0; i < 16; i++)
            hesapNo += random.Next(0, 10).ToString();
        return $"TR{kontrolHanesi}0001{hesapNo}";
    }
}
