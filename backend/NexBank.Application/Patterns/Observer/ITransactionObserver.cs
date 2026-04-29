using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Observer;

/// <summary>
/// OBSERVER PATTERN
/// Bir işlem onaylandığında tetiklenecek abonelerin uygulayacağı arayüz.
/// </summary>
public interface ITransactionObserver
{
    Task UpdateAsync(Transaction transaction);
}
