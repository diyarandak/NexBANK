using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface IReportService
{
    Task<List<TransactionDto>> GetAccountStatementAsync(int accountId, DateTime? startDate, DateTime? endDate);
    Task<object> GetSpendingAnalysisAsync(int userId, DateTime? startDate, DateTime? endDate);
    Task<string> GenerateCsvStatementAsync(int accountId, DateTime? startDate, DateTime? endDate);
}
