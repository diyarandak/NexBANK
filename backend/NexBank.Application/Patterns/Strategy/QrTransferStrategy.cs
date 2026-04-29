using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class QrTransferStrategy : IPaymentStrategy
{
    public PaymentMethod Method => PaymentMethod.QRTransfer;

    public decimal CalculateFee(decimal amount)
    {
        // QR transfer teşvik edildiği için ilk 1000₺ altı masrafsız, sonrası sabit 2₺
        return amount <= 1000m ? 0 : 2m;
    }
}
