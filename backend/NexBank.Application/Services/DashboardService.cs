using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Interfaces;
using NexBank.Core.Entities;

namespace NexBank.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ILoanRepository _loanRepository;

    public DashboardService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ICreditCardRepository creditCardRepository,
        ILoanRepository loanRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _creditCardRepository = creditCardRepository;
        _loanRepository = loanRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync(int userId, string role)
    {
        decimal totalBalance = 0;
        int activeAccounts = 0;
        decimal todayVolume = 0;
        int pendingCount = 0;
        List<TransactionDto> recentTransactions = new();

        if (role == "Admin")
        {
            totalBalance = await _accountRepository.GetTotalBalanceAsync();
            activeAccounts = await _accountRepository.GetCountAsync();
            todayVolume = await _transactionRepository.GetTodayVolumeAsync();
            pendingCount = (await _transactionRepository.GetPendingAsync()).Count;

            var allTransactions = await _transactionRepository.GetAllAsync();
            recentTransactions = allTransactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(MapTransactionToDto)
                .ToList();

            // Banka Genel Borç ve Kredi Özetleri
            var allCards = await _creditCardRepository.GetAllAsync();
            var allLoans = await _loanRepository.GetAllAsync();
            
            var creditCardDebt = allCards.Sum(c => c.UsedAmount);
            var activeLoansTotal = allLoans.Where(l => l.Status == LoanStatus.Approved).Sum(l => l.RemainingBalance);

            return new DashboardDto
            {
                TotalBalance = totalBalance,
                ActiveAccounts = activeAccounts,
                TodayVolume = todayVolume,
                PendingTransactions = pendingCount,
                CreditCardDebt = creditCardDebt,
                ActiveLoansTotal = activeLoansTotal,
                RecentTransactions = recentTransactions
            };
        }
        else
        {
            var userAccounts = await _accountRepository.GetByUserIdAsync(userId);
            totalBalance = userAccounts.Where(a => a.IsActive).Sum(a => a.Balance);
            activeAccounts = userAccounts.Count(a => a.IsActive);

            var accountIds = userAccounts.Select(a => a.Id).ToHashSet();

            var allTransactions = await _transactionRepository.GetAllAsync();
            var userTransactions = allTransactions
                .Where(t => (t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value)) || 
                            (t.ToAccountId.HasValue && accountIds.Contains(t.ToAccountId.Value)))
                .ToList();

            todayVolume = userTransactions
                .Where(t => t.CreatedAt.Date == DateTime.UtcNow.Date)
                .Sum(t => t.Amount);

            pendingCount = userTransactions.Count(t => t.Status == Core.Entities.TransactionStatus.Pending);

            recentTransactions = userTransactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(MapTransactionToDto)
                .ToList();

            // Borç ve Kredi Hesaplamaları
            var cards = await _creditCardRepository.GetByUserIdAsync(userId);
            var loans = await _loanRepository.GetByUserIdAsync(userId);
            
            var creditCardDebt = cards.Sum(c => c.UsedAmount);
            var activeLoansTotal = loans.Where(l => l.Status == LoanStatus.Approved).Sum(l => l.RemainingBalance);

            return new DashboardDto
            {
                TotalBalance = totalBalance,
                ActiveAccounts = activeAccounts,
                TodayVolume = todayVolume,
                PendingTransactions = pendingCount,
                CreditCardDebt = creditCardDebt,
                ActiveLoansTotal = activeLoansTotal,
                RecentTransactions = recentTransactions
            };
        }
    }

    private static TransactionDto MapTransactionToDto(Core.Entities.Transaction t)
    {
        return new TransactionDto
        {
            Id = t.Id,
            FromAccountId = t.FromAccountId,
            ToAccountId = t.ToAccountId,
            Amount = t.Amount,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            ProcessedAt = t.ProcessedAt
        };
    }
}
