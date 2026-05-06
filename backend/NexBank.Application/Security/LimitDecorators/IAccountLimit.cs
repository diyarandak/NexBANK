namespace NexBank.Application.Security.LimitDecorators;

/// <summary>
/// DECORATOR PATTERN - Component
/// Hesap özellikleri ve limitleri için üst arayüz.
/// </summary>
public interface IAccountLimit
{
    decimal GetDailyLimit();
    string GetFeatures();
}
