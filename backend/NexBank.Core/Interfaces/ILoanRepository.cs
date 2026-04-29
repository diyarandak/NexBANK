using NexBank.Core.Entities;

namespace NexBank.Core.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<List<Loan>> GetByUserIdAsync(int userId);
    Task<List<Loan>> GetAllAsync();
    Task<List<Loan>> GetAllPendingAsync();
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task AddPaymentAsync(LoanPayment payment);
    Task UpdatePaymentAsync(LoanPayment payment);
    Task<LoanPayment?> GetPaymentByIdAsync(int paymentId);
}
