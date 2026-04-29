using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface ISupportRepository
{
    Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
    Task<Notification?> GetNotificationByIdAsync(int id);
    Task AddNotificationAsync(Notification notification);
    Task UpdateNotificationAsync(Notification notification);

    Task<List<Ticket>> GetTicketsByUserIdAsync(int userId);
    Task AddTicketAsync(Ticket ticket);
}
