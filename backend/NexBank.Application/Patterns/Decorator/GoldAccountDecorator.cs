namespace NexBank.Application.Patterns.Decorator;

/// <summary>
/// DECORATOR PATTERN - Concrete Decorator
/// Temel limitin üzerine +20,000 ekler, ekstra özellik koyar.
/// </summary>
public class GoldAccountDecorator : AccountLimitDecorator
{
    public GoldAccountDecorator(IAccountLimit accountLimit) : base(accountLimit)
    {
    }

    public override decimal GetDailyLimit()
    {
        return base.GetDailyLimit() + 20_000m;
    }

    public override string GetFeatures()
    {
        return base.GetFeatures() + ", Ücretsiz EFT/Havale Masrafı, Gold Müşteri Hattı";
    }
}
