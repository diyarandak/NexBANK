using System.Text;
using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    
    public ReportService(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task<List<TransactionDto>> GetAccountStatementAsync(int accountId, DateTime? startDate, DateTime? endDate)
    {
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
        
        var query = transactions.AsQueryable();
        if (startDate.HasValue) query = query.Where(t => t.CreatedAt.Date >= startDate.Value.Date);
        if (endDate.HasValue) query = query.Where(t => t.CreatedAt.Date <= endDate.Value.Date);

        return query.Select(t => new TransactionDto
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
        }).ToList();
    }

    public async Task<object> GetSpendingAnalysisAsync(int userId, DateTime? startDate, DateTime? endDate)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(userId);
        var accountIds = accounts.Select(a => a.Id).ToList();

        // Harcamaları gruplandır (Giden transferler, fatura ödemeleri vs. genelde withdrawal veya transfer olarak giden kısımlar)
        var allTransactions = await _transactionRepository.GetAllAsync();
        
        var userSpends = allTransactions.Where(t => t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value) && t.FromAccountId != t.ToAccountId && t.Status == Core.Entities.TransactionStatus.Approved).ToList();

        if (startDate.HasValue) userSpends = userSpends.Where(t => t.CreatedAt.Date >= startDate.Value.Date).ToList();
        if (endDate.HasValue) userSpends = userSpends.Where(t => t.CreatedAt.Date <= endDate.Value.Date).ToList();

        var categories = userSpends.GroupBy(t => t.Type.ToString())
            .Select(g => new {
                Category = g.Key,
                TotalAmount = g.Sum(t => t.Amount),
                Count = g.Count()
            }).ToList();
            
        var totalSpent = categories.Sum(c => c.TotalAmount);

        return new {
            TotalSpent = totalSpent,
            Categories = categories
        };
    }

    public async Task<string> GenerateCsvStatementAsync(int accountId, DateTime? startDate, DateTime? endDate)
    {
        var statement = await GetAccountStatementAsync(accountId, startDate, endDate);
        
        var sb = new StringBuilder();
        sb.AppendLine("Tarih,Islem Tipi,Tutar,Aciklama,Durum");

        foreach (var item in statement)
        {
            var pAmount = item.FromAccountId == accountId ? "-" + item.Amount.ToString() : "+" + item.Amount.ToString();
            sb.AppendLine($"{item.CreatedAt:dd/MM/yyyy HH:mm},{item.Type},{pAmount},\"{item.Description}\",{item.Status}");
        }

        return sb.ToString();
    }
}
