using System.ComponentModel.DataAnnotations;

namespace NexBank.Core.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StaffResponse { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}

public enum TicketStatus { Open, InProgress, Resolved, Closed }
public enum TicketPriority { Low, Normal, High, Urgent }
