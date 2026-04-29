using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.Strategy;

public class PaymentStrategyContext
{
    private readonly IEnumerable<IPaymentStrategy> _strategies;

    public PaymentStrategyContext(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies;
    }

    public decimal CalculateFee(PaymentMethod method, decimal amount)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Method == method);
        if (strategy == null)
            throw new NotSupportedException($"Ödeme yöntemi desteklenmiyor: {method}");

        return strategy.CalculateFee(amount);
    }
}
