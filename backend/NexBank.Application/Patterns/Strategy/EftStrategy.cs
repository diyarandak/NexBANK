using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class EftStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.EFT;

    public decimal CalculateFee(decimal amount)
    {
        // EFT için sabit binde 2 masraf (min 3₺)
        decimal fee = amount * 0.002m;
        return fee < 3m ? 3m : fee;
    }
}
