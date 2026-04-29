using System.ComponentModel.DataAnnotations;

namespace NexBank.Core.Entities;

public class FavoriteRecipient
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string RecipientName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(26)]
    public string RecipientIban { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Label { get; set; } // Örn: "Kira", "Maaş", "Arkadaş"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
