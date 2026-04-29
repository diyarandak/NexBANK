using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Observer;

/// <summary>
/// OBSERVER PATTERN - Subject (Yayıncı)
/// İşlem gerçekleştiğinde aboneleri bilgilendirir.
/// </summary>
public class TransactionSubject
{
    private readonly List<ITransactionObserver> _observers = new();

    public void Attach(ITransactionObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Detach(ITransactionObserver observer)
    {
        _observers.Remove(observer);
    }

    public async Task NotifyAsync(Transaction transaction)
    {
        foreach (var observer in _observers)
        {
            await observer.UpdateAsync(transaction);
        }
    }
}
