namespace NexBank.Core.Entities;

/// <summary>
/// Kredi başvurusu — Müşteri başvurur, Personel onaylar/reddeder.
/// </summary>
public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } // Aylık faiz (örn: 0.025 = %2.5)
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal RemainingBalance { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByStaffId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public User? ApprovedByStaff { get; set; }
    public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
}
