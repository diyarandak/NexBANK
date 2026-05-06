namespace NexBank.Application.Security.LimitDecorators;

/// <summary>
/// DECORATOR PATTERN - Concrete Component
/// Temel hesap limiti ve özellikleri. 
/// Factory ile üretilen temel nesne etrafına sarılabilir.
/// </summary>
public class BaseAccountLimit : IAccountLimit
{
    private readonly decimal _baseLimit;

    public BaseAccountLimit(decimal baseLimit)
    {
        _baseLimit = baseLimit;
    }

    public decimal GetDailyLimit()
    {
        return _baseLimit;
    }

    public string GetFeatures()
    {
        return "Temel Bankacılık İşlemleri";
    }
}
