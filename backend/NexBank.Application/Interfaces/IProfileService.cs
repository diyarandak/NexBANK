using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IProfileService
{
    Task<UserDto?> GetProfileAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, string fullName, string email);
    
    // Notifications
    Task<object> GetNotificationsAsync(int userId);
    Task MarkNotificationAsReadAsync(int notificationId, int userId);

    // Support Tickets
    Task<object> GetTicketsAsync(int userId);
    Task<object> CreateTicketAsync(int userId, string subject, string message);
}
