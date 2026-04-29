using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class BillRepository : IBillRepository
{
    private readonly NexBankDbContext _context;
    public BillRepository(NexBankDbContext context) => _context = context;

    public async Task<Bill?> GetByIdAsync(int id) => await _context.Bills.FindAsync(id);
    
    public async Task<List<Bill>> GetByUserIdAsync(int userId) => 
        await _context.Bills.Where(b => b.UserId == userId).OrderBy(b => b.DueDate).ToListAsync();

    public async Task<Bill> AddAsync(Bill bill)
    {
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task UpdateAsync(Bill bill)
    {
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();
    }
}

public class FavoriteRecipientRepository : IFavoriteRecipientRepository
{
    private readonly NexBankDbContext _context;
    public FavoriteRecipientRepository(NexBankDbContext context) => _context = context;

    public async Task<FavoriteRecipient?> GetByIdAsync(int id) => await _context.FavoriteRecipients.FindAsync(id);
    
    public async Task<List<FavoriteRecipient>> GetByUserIdAsync(int userId) => 
        await _context.FavoriteRecipients.Where(f => f.UserId == userId).ToListAsync();

    public async Task<FavoriteRecipient> AddAsync(FavoriteRecipient recipient)
    {
        _context.FavoriteRecipients.Add(recipient);
        await _context.SaveChangesAsync();
        return recipient;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.FavoriteRecipients.FindAsync(id);
        if (item != null)
        {
            _context.FavoriteRecipients.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class StandingOrderRepository : IStandingOrderRepository
{
    private readonly NexBankDbContext _context;
    public StandingOrderRepository(NexBankDbContext context) => _context = context;

    public async Task<StandingOrder?> GetByIdAsync(int id) => await _context.StandingOrders.FindAsync(id);
    
    public async Task<List<StandingOrder>> GetByUserIdAsync(int userId) => 
        await _context.StandingOrders.Where(s => s.UserId == userId).ToListAsync();

    public async Task<List<StandingOrder>> GetPendingOrdersAsync(DateTime date) =>
        await _context.StandingOrders
            .Where(s => s.IsActive && s.NextPaymentDate.Date <= date.Date)
            .ToListAsync();

    public async Task<StandingOrder> AddAsync(StandingOrder order)
    {
        _context.StandingOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(StandingOrder order)
    {
        _context.StandingOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.StandingOrders.FindAsync(id);
        if (item != null)
        {
            _context.StandingOrders.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

public class NotificationRepository : INotificationRepository
{
    private readonly NexBankDbContext _context;
    public NotificationRepository(NexBankDbContext context) => _context = context;

    public async Task<List<Notification>> GetByUserIdAsync(int userId) =>
        await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<Notification> AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
