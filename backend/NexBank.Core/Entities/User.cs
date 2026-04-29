namespace NexBank.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty; // Müşteriler TC ile giriş yapar
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Branch { get; set; } = "Genel Müdürlük"; // Default şube
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
