using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class SwiftStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.SWIFT;

    public decimal CalculateFee(decimal amount)
    {
        // SWIFT (Yurtdışı) için sabit %1 komisyon
        return amount * 0.01m;
    }
}
