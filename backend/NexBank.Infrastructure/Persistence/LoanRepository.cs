using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class LoanRepository : ILoanRepository
{
    private readonly NexBankDbContext _context;

    public LoanRepository(NexBankDbContext context) => _context = context;

    public async Task<Loan?> GetByIdAsync(int id) =>
        await _context.Loans
            .Include(l => l.Payments)
            .Include(l => l.Account)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<List<Loan>> GetByUserIdAsync(int userId) =>
        await _context.Loans
            .Include(l => l.Payments)
            .Include(l => l.Account)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<List<Loan>> GetAllAsync() =>
        await _context.Loans.ToListAsync();

    public async Task<List<Loan>> GetAllPendingAsync() =>
        await _context.Loans
            .Include(l => l.User)
            .Where(l => l.Status == LoanStatus.Pending)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

    public async Task<Loan> AddAsync(Loan loan)
    {
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }

    public async Task AddPaymentAsync(LoanPayment payment)
    {
        _context.LoanPayments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePaymentAsync(LoanPayment payment)
    {
        _context.LoanPayments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<LoanPayment?> GetPaymentByIdAsync(int paymentId) =>
        await _context.LoanPayments
            .Include(p => p.Loan)
            .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
}
