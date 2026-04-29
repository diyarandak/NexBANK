using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class HavaleStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.Havale;

    public decimal CalculateFee(decimal amount)
    {
        // Havale şirket içi olduğu için masrafsız
        return 0;
    }
}
