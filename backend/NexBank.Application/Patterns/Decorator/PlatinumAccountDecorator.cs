namespace NexBank.Application.Patterns.Decorator;

/// <summary>
/// DECORATOR PATTERN - Concrete Decorator
/// Temel limitin üzerine +100,000 ekler.
/// </summary>
public class PlatinumAccountDecorator : AccountLimitDecorator
{
    public PlatinumAccountDecorator(IAccountLimit accountLimit) : base(accountLimit)
    {
    }

    public override decimal GetDailyLimit()
    {
        return base.GetDailyLimit() + 100_000m;
    }

    public override string GetFeatures()
    {
        return base.GetFeatures() + ", Özel Müşteri Temsilcisi, Lounge Hizmeti, %50 İndirimli Kredi";
    }
}
