using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Factory;

/// <summary>
/// FACTORY PATTERN — Interface
/// Farklı tipte banka hesapları oluşturmak için fabrika arayüzü.
/// Her hesap tipi farklı başlangıç limitleri ve özellikleri ile üretilir.
/// </summary>
public interface IAccountFactory
{
    Account CreateAccount(AccountType type, int userId, string currency = "TRY");
}
