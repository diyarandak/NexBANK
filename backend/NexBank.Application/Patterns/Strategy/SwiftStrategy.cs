using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class SwiftStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.SWIFT;

    public decimal CalculateFee(decimal amount)
    {
        // Dinamik SWIFT: %1.5 + 250 TL Sabit. Min 500 TL.
        decimal fee = (amount * 0.015m) + 250m;
        return fee < 500m ? 500m : fee;
    }
}
