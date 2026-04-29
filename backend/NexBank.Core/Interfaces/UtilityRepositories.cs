using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(int id);
    Task<List<Bill>> GetByUserIdAsync(int userId);
    Task<Bill> AddAsync(Bill bill);
    Task UpdateAsync(Bill bill);
}

public interface IFavoriteRecipientRepository
{
    Task<FavoriteRecipient?> GetByIdAsync(int id);
    Task<List<FavoriteRecipient>> GetByUserIdAsync(int userId);
    Task<FavoriteRecipient> AddAsync(FavoriteRecipient recipient);
    Task DeleteAsync(int id);
}

public interface IStandingOrderRepository
{
    Task<StandingOrder?> GetByIdAsync(int id);
    Task<List<StandingOrder>> GetByUserIdAsync(int userId);
    Task<List<StandingOrder>> GetPendingOrdersAsync(DateTime date); // O gün ödenmesi gerekenler
    Task<StandingOrder> AddAsync(StandingOrder order);
    Task UpdateAsync(StandingOrder order);
    Task DeleteAsync(int id);
}

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<Notification> AddAsync(Notification notification);
    Task MarkAsReadAsync(int id);
}
