using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IBillService
{
    Task<List<BillDto>> GetUserBillsAsync(int userId);
    Task<BillDto> CreateBillAsync(int userId, CreateBillDto dto); // Simülasyon için kendi faturalarını üretme aracı
    Task<bool> PayBillAsync(int userId, PayBillDto dto);
}

public interface IFavoriteRecipientService
{
    Task<List<FavoriteRecipientDto>> GetUserFavoritesAsync(int userId);
    Task<FavoriteRecipientDto> AddFavoriteAsync(int userId, CreateFavoriteDto dto);
    Task<bool> RemoveFavoriteAsync(int id, int userId);
}

public interface IStandingOrderService
{
    Task<List<StandingOrderDto>> GetUserStandingOrdersAsync(int userId);
    Task<StandingOrderDto> AddStandingOrderAsync(int userId, CreateStandingOrderDto dto);
    Task<bool> RemoveStandingOrderAsync(int id, int userId);
    Task ProcessPendingOrdersAsync(DateTime date); // Otomatik ödemeleri çalıştıracak arka plan metodu
}
