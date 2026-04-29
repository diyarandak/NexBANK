namespace NexBank.Core.Entities;

/// <summary>
/// Kredi kartı harcaması / nakit avans
/// </summary>
public class CreditCardTransaction
{
    public int Id { get; set; }
    public int CreditCardId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Alışveriş, Fatura, NakitAvans, vb.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public CreditCard CreditCard { get; set; } = null!;
}
