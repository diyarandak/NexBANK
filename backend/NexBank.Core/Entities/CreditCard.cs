namespace NexBank.Core.Entities;

/// <summary>
/// Sanal Kredi Kartı
/// </summary>
public class CreditCard
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CardNumber { get; set; } = string.Empty; // 16 hane
    public string Cvv { get; set; } = string.Empty; // 3 hane
    public string ExpiryDate { get; set; } = string.Empty; // MM/YY
    public decimal CardLimit { get; set; } = 10_000m;
    public decimal UsedAmount { get; set; } = 0m;
    public decimal AvailableLimit => CardLimit - UsedAmount;
    public decimal MinPayment => UsedAmount * 0.20m; // Borçun %20'si
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<CreditCardTransaction> Transactions { get; set; } = new List<CreditCardTransaction>();
}
