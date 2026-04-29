using System.ComponentModel.DataAnnotations;
using NexBank.Core.Entities;

namespace NexBank.Core.Entities;

public class SavingGoal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string Category { get; set; } = "Diğer"; // Tatil, Araba, Ev, Teknoloji, vb.
    public DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
