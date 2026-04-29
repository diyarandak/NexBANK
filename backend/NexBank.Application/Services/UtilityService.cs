using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class UtilityService : IBillService, IFavoriteRecipientService, IStandingOrderService
{
    private readonly IBillRepository _billRepository;
    private readonly IFavoriteRecipientRepository _favoriteRecipientRepository;
    private readonly IStandingOrderRepository _standingOrderRepository;
    private readonly ITransactionService _transactionService;
    private readonly IAccountRepository _accountRepository;

    public UtilityService(
        IBillRepository billRepository, 
        IFavoriteRecipientRepository favoriteRecipientRepository, 
        IStandingOrderRepository standingOrderRepository,
        ITransactionService transactionService,
        IAccountRepository accountRepository)
    {
        _billRepository = billRepository;
        _favoriteRecipientRepository = favoriteRecipientRepository;
        _standingOrderRepository = standingOrderRepository;
        _transactionService = transactionService;
        _accountRepository = accountRepository;
    }

    // ── BILLS ──
    public async Task<List<BillDto>> GetUserBillsAsync(int userId)
    {
        var bills = await _billRepository.GetByUserIdAsync(userId);
        return bills.Select(b => new BillDto
        {
            Id = b.Id, BillType = b.BillType.ToString(), InstitutionName = b.InstitutionName,
            SubscriberNo = b.SubscriberNo, Amount = b.Amount, DueDate = b.DueDate,
            IsPaid = b.IsPaid, PaidAt = b.PaidAt
        }).ToList();
    }

    public async Task<BillDto> CreateBillAsync(int userId, CreateBillDto dto)
    {
        if (!Enum.TryParse<BillType>(dto.BillType, true, out var billType))
            billType = BillType.Elektrik;

        var bill = new Bill
        {
            UserId = userId, BillType = billType, InstitutionName = dto.InstitutionName,
            SubscriberNo = dto.SubscriberNo, Amount = dto.Amount, DueDate = dto.DueDate,
            IsPaid = false
        };
        var saved = await _billRepository.AddAsync(bill);
        return new BillDto { Id = saved.Id, BillType = saved.BillType.ToString(), InstitutionName = saved.InstitutionName, SubscriberNo = saved.SubscriberNo, Amount = saved.Amount, DueDate = saved.DueDate, IsPaid = saved.IsPaid };
    }

    public async Task<bool> PayBillAsync(int userId, PayBillDto dto)
    {
        var bill = await _billRepository.GetByIdAsync(dto.BillId);
        if (bill == null || bill.UserId != userId || bill.IsPaid) return false;

        var account = await _accountRepository.GetByIdAsync(dto.AccountId);
        if (account == null || account.UserId != userId || account.Balance < bill.Amount) return false;

        bool success = await _transactionService.MakeWithdrawAsync(account.Id, bill.Amount);
        if (success)
        {
            bill.IsPaid = true;
            bill.PaidAt = DateTime.UtcNow;
            await _billRepository.UpdateAsync(bill);
            return true;
        }
        return false;
    }

    // ── FAVORITES ──
    public async Task<List<FavoriteRecipientDto>> GetUserFavoritesAsync(int userId)
    {
        var favs = await _favoriteRecipientRepository.GetByUserIdAsync(userId);
        return favs.Select(f => new FavoriteRecipientDto
        {
            Id = f.Id, RecipientIban = f.RecipientIban, Label = f.Label, RecipientName = f.RecipientName
        }).ToList();
    }

    public async Task<FavoriteRecipientDto> AddFavoriteAsync(int userId, CreateFavoriteDto dto)
    {
        var fav = new FavoriteRecipient
        {
            UserId = userId, RecipientIban = dto.RecipientIban, Label = dto.Label, RecipientName = dto.RecipientName, CreatedAt = DateTime.UtcNow
        };
        var saved = await _favoriteRecipientRepository.AddAsync(fav);
        return new FavoriteRecipientDto { Id = saved.Id, Label = saved.Label, RecipientIban = saved.RecipientIban, RecipientName = saved.RecipientName };
    }

    public async Task<bool> RemoveFavoriteAsync(int id, int userId)
    {
        var fav = await _favoriteRecipientRepository.GetByIdAsync(id);
        if (fav == null || fav.UserId != userId) return false;
        await _favoriteRecipientRepository.DeleteAsync(id);
        return true;
    }

    // ── STANDING ORDERS ──
    public async Task<List<StandingOrderDto>> GetUserStandingOrdersAsync(int userId)
    {
        var orders = await _standingOrderRepository.GetByUserIdAsync(userId);
        return orders.Select(o => new StandingOrderDto
        {
            Id = o.Id, FromAccountId = o.FromAccountId, ToIban = o.ToIban, Amount = o.Amount, 
            RecipientName = o.RecipientName, Frequency = o.Frequency, Description = o.Description,
            NextPaymentDate = o.NextPaymentDate, EndDate = o.EndDate, IsActive = o.IsActive
        }).ToList();
    }

    public async Task<StandingOrderDto> AddStandingOrderAsync(int userId, CreateStandingOrderDto dto)
    {
        // Account validation
        var account = await _accountRepository.GetByIdAsync(dto.FromAccountId);
        if (account == null || account.UserId != userId) throw new ArgumentException("Geçersiz kaynak hesap.");

        var order = new StandingOrder
        {
            UserId = userId, FromAccountId = dto.FromAccountId, ToIban = dto.ToIban, Amount = dto.Amount, 
            RecipientName = dto.RecipientName, Frequency = dto.Frequency, Description = dto.Description,
            NextPaymentDate = dto.NextPaymentDate, EndDate = dto.EndDate, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        var saved = await _standingOrderRepository.AddAsync(order);
        return new StandingOrderDto
        {
             Id = saved.Id, FromAccountId = saved.FromAccountId, ToIban = saved.ToIban, Amount = saved.Amount, 
             RecipientName = saved.RecipientName, Frequency = saved.Frequency, Description = saved.Description,
             NextPaymentDate = saved.NextPaymentDate, EndDate = saved.EndDate, IsActive = saved.IsActive
        };
    }

    public async Task<bool> RemoveStandingOrderAsync(int id, int userId)
    {
        var order = await _standingOrderRepository.GetByIdAsync(id);
        if (order == null || order.UserId != userId) return false;
        await _standingOrderRepository.DeleteAsync(id);
        return true;
    }

    public async Task ProcessPendingOrdersAsync(DateTime date)
    {
        var pendingOrders = await _standingOrderRepository.GetPendingOrdersAsync(date);
        foreach(var order in pendingOrders)
        {
            try
            {
                var success = await _transactionService.MakeTransferAsync(order.FromAccountId, order.ToIban, order.Amount, "EFT");
                if (success)
                {
                    if (order.Frequency == "Aylık")
                        order.NextPaymentDate = order.NextPaymentDate.AddMonths(1);
                    else
                        order.NextPaymentDate = order.NextPaymentDate.AddDays(7);

                    await _standingOrderRepository.UpdateAsync(order);
                }
            }
            catch
            {
                // Bir hata olursa loglanabilir. Otomatik işlem olduğu için kesinti yapılmaz.
            }
        }
    }
}
