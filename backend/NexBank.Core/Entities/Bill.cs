namespace NexBank.Core.Entities;

/// <summary>
/// Fatura — Elektrik, su, doğalgaz, internet, telefon
/// </summary>
public class Bill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public BillType BillType { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string SubscriberNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    public int? PaidFromAccountId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Account? PaidFromAccount { get; set; }
}
