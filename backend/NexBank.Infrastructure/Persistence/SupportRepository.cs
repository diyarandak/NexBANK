using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class SupportRepository : ISupportRepository
{
    private readonly NexBankDbContext _context;
    public SupportRepository(NexBankDbContext context) => _context = context;

    public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId) =>
        await _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();

    public async Task<Notification?> GetNotificationByIdAsync(int id) => await _context.Notifications.FindAsync(id);

    public async Task AddNotificationAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateNotificationAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Ticket>> GetTicketsByUserIdAsync(int userId) =>
        await _context.Tickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task AddTicketAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
    }
}
