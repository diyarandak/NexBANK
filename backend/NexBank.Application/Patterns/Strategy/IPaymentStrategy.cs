using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

/// <summary>
/// STRATEGY PATTERN
/// Ödeme yöntemine göre kesinti/komisyon hesaplayan arayüz.
/// </summary>
public interface IPaymentStrategy
{
    PaymentMethod Method { get; }
    decimal CalculateFee(decimal amount);
}
