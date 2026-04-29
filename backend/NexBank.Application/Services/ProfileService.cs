using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ISupportRepository _supportRepository;

    public ProfileService(IUserRepository userRepository, ISupportRepository supportRepository)
    {
        _userRepository = userRepository;
        _supportRepository = supportRepository;
    }

    public async Task<UserDto?> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;
        return new UserDto 
        { 
            Id = user.Id, 
            FullName = user.FullName, 
            TcKimlikNo = user.TcKimlikNo, 
            Email = user.Email ?? "", 
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;
        user.FullName = fullName;
        user.Email = email;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<object> GetNotificationsAsync(int userId)
    {
        return await _supportRepository.GetNotificationsByUserIdAsync(userId);
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, int userId)
    {
        var notif = await _supportRepository.GetNotificationByIdAsync(notificationId);
        if (notif != null && notif.UserId == userId)
        {
            notif.IsRead = true;
            await _supportRepository.UpdateNotificationAsync(notif);
        }
    }

    public async Task<object> GetTicketsAsync(int userId)
    {
        return await _supportRepository.GetTicketsByUserIdAsync(userId);
    }

    public async Task<object> CreateTicketAsync(int userId, string subject, string message)
    {
        var ticket = new Ticket { UserId = userId, Subject = subject, Message = message, CreatedAt = DateTime.UtcNow };
        await _supportRepository.AddTicketAsync(ticket);
        return ticket;
    }
}
