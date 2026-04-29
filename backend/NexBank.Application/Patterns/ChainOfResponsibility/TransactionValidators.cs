using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.ChainOfResponsibility;

/// <summary>
/// CHAIN OF RESPONSIBILITY PATTERN: İşlem validasyonlarını bir zincir halinde yapar.
/// Her halka sadece kendi sorumluluğunu kontrol eder.
/// </summary>
public abstract class TransactionHandler
{
    protected TransactionHandler? NextHandler;

    public void SetNext(TransactionHandler handler)
    {
        NextHandler = handler;
    }

    public abstract Task HandleAsync(Account account, decimal amount);
}

public class BalanceCheckHandler : TransactionHandler
{
    public override async Task HandleAsync(Account account, decimal amount)
    {
        if (account.Balance < amount)
            throw new Exception("Yetersiz bakiye. İşlem reddedildi.");

        if (NextHandler != null)
            await NextHandler.HandleAsync(account, amount);
    }
}

public class DailyLimitHandler : TransactionHandler
{
    public override async Task HandleAsync(Account account, decimal amount)
    {
        if (amount > account.DailyLimit)
            throw new Exception($"Günlük işlem limitini ({account.DailyLimit}₺) aştınız.");

        if (NextHandler != null)
            await NextHandler.HandleAsync(account, amount);
    }
}

public class AccountStatusHandler : TransactionHandler
{
    public override async Task HandleAsync(Account account, decimal amount)
    {
        if (!account.IsActive)
            throw new Exception("Hesap pasif durumda. İşlem yapılamaz.");

        if (NextHandler != null)
            await NextHandler.HandleAsync(account, amount);
    }
}
