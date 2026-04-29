using NexBank.Core.Entities;

namespace NexBank.Application.Patterns.State;

public interface ILoanState
{
    void Approve(Loan loan);
    void Reject(Loan loan);
    void Pay(Loan loan, decimal amount);
}

public class PendingState : ILoanState
{
    public void Approve(Loan loan)
    {
        loan.Status = LoanStatus.Approved;
    }

    public void Reject(Loan loan)
    {
        loan.Status = LoanStatus.Rejected;
    }

    public void Pay(Loan loan, decimal amount)
    {
        throw new Exception("Onaylanmamış bir krediye ödeme yapılamaz.");
    }
}

public class ApprovedState : ILoanState
{
    public void Approve(Loan loan) => throw new Exception("Kredi zaten onaylı.");
    public void Reject(Loan loan) => throw new Exception("Onaylanmış kredi reddedilemez.");

    public void Pay(Loan loan, decimal amount)
    {
        loan.RemainingBalance -= amount;
        if (loan.RemainingBalance <= 0)
        {
            loan.RemainingBalance = 0;
            loan.Status = LoanStatus.Completed;
        }
    }
}

public class CompletedState : ILoanState
{
    public void Approve(Loan loan) => throw new Exception("Kredi zaten tamamlanmış.");
    public void Reject(Loan loan) => throw new Exception("Kredi zaten tamamlanmış.");
    public void Pay(Loan loan, decimal amount) => throw new Exception("Tamamlanmış krediye ödeme yapılamaz.");
}
