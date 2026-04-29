namespace NexBank.Core.Entities;

/// <summary>
/// Otomatik ödeme talimatı
/// </summary>
public class StandingOrder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FromAccountId { get; set; }
    public string ToIban { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "Aylık"; // Aylık, Haftalık
    public string Description { get; set; } = string.Empty;
    public DateTime NextPaymentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Account FromAccount { get; set; } = null!;
}
