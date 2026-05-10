using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class EftStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.EFT;

    public decimal CalculateFee(decimal amount)
    {
        // Hassas EFT Hesabı: (Tutar * 0.004) - 80. Minimum 15 TL.
        decimal calculated = (amount * 0.004m) - 80m;
        return calculated < 15m ? 15m : calculated;
    }
}
