namespace NexBank.Application.Patterns.Decorator;

/// <summary>
/// DECORATOR PATTERN - Base Decorator
/// </summary>
public abstract class AccountLimitDecorator : IAccountLimit
{
    protected readonly IAccountLimit _accountLimit;

    protected AccountLimitDecorator(IAccountLimit accountLimit)
    {
        _accountLimit = accountLimit;
    }

    public virtual decimal GetDailyLimit()
    {
        return _accountLimit.GetDailyLimit();
    }

    public virtual string GetFeatures()
    {
        return _accountLimit.GetFeatures();
    }
}
