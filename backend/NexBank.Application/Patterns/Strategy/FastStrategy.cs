using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class FastStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.FAST;

    public decimal CalculateFee(decimal amount)
    {
        // Dinamik FAST: Binde 1 + 2 TL Sabit. (Min 5, Max 100)
        decimal fee = (amount * 0.001m) + 2m;
        if (fee < 5m) return 5m;
        if (fee > 100m) return 100m;
        return fee;
    }
}
