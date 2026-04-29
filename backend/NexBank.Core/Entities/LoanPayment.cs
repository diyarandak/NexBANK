namespace NexBank.Core.Entities;

/// <summary>
/// Kredi taksit ödemesi
/// </summary>
public class LoanPayment
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsPaid { get; set; } = false;

    // Navigation
    public Loan Loan { get; set; } = null!;
}
