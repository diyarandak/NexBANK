using NexBank.Application.DTOs;

namespace NexBank.Application.Interfaces;

public interface ILoanService
{
    Task<List<LoanDto>> GetUserLoansAsync(int userId);
    Task<List<LoanDto>> GetPendingLoansAsync();
    Task<LoanDto> ApplyForLoanAsync(int userId, LoanApplicationDto dto);
    Task<bool> ApproveLoanAsync(int loanId, int staffId);
    Task<bool> RejectLoanAsync(int loanId, int staffId);
    Task<bool> PayInstallmentAsync(int paymentId, int accountId);
}
