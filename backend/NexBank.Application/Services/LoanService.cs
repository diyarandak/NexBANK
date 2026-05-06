using NexBank.Application.DTOs;
using NexBank.Application.Interfaces;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionService _transactionService;
    private readonly INotificationRepository _notificationRepository;

    public LoanService(
        ILoanRepository loanRepository, 
        IAccountRepository accountRepository, 
        ITransactionService transactionService,
        INotificationRepository notificationRepository)
    {
        _loanRepository = loanRepository;
        _accountRepository = accountRepository;
        _transactionService = transactionService;
        _notificationRepository = notificationRepository;
    }

    public async Task<List<LoanDto>> GetUserLoansAsync(int userId)
    {
        var loans = await _loanRepository.GetByUserIdAsync(userId);
        return loans.Select(MapToDto).ToList();
    }

    public async Task<List<LoanDto>> GetPendingLoansAsync()
    {
        var loans = await _loanRepository.GetAllPendingAsync();
        return loans.Select(MapToDto).ToList();
    }

    public async Task<LoanDto> ApplyForLoanAsync(int userId, LoanApplicationDto dto)
    {
        // Frontend'den gelen değerleri enum ile eşleştir (Alias desteği ekle)
        string typeStr = dto.LoanType;
        if (typeStr == "Personal") typeStr = "Ihtiyac";
        else if (typeStr == "Home") typeStr = "Konut";
        else if (typeStr == "Car") typeStr = "Tasit";

        if (!Enum.TryParse<LoanType>(typeStr, true, out var loanType))
            throw new ArgumentException("Geçersiz kredi türü. Lütfen İhtiyaç, Konut veya Taşıt kredisi seçin.");

        decimal interestRate = loanType switch
        {
            LoanType.Ihtiyac => 0.035m, // %3.5 aylık
            LoanType.Konut => 0.025m,   // %2.5 aylık
            LoanType.Tasit => 0.029m,   // %2.9 aylık
            _ => 0.04m,
        };

        // Standart PMT (Payment) faiz taksit hesabı formülü: Taksit = (Anapara x Faiz) / (1 - (1+Faiz)^-Vade)
        double r = (double)interestRate;
        double n = dto.TermMonths;
        double p = (double)dto.Amount;
        double math1 = Math.Pow(1 + r, -n);
        double monthlyPayment = (p * r) / (1 - math1);
        
        decimal monthlyPaymentDec = Math.Round((decimal)monthlyPayment, 2);
        decimal totalPayment = monthlyPaymentDec * dto.TermMonths;

        var loan = new Loan
        {
            UserId = userId,
            AccountId = dto.AccountId,
            LoanType = loanType,
            Amount = dto.Amount,
            InterestRate = interestRate,
            TermMonths = dto.TermMonths,
            MonthlyPayment = monthlyPaymentDec,
            TotalPayment = totalPayment,
            RemainingBalance = totalPayment,
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var savedLoan = await _loanRepository.AddAsync(loan);
        return MapToDto(savedLoan);
    }

    public async Task<bool> ApproveLoanAsync(int loanId, int staffId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null || loan.Status != LoanStatus.Pending) return false;

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.ApprovedByStaffId = staffId;

        // Ödeme planı çıkar (Taksitleri oluştur)
        for (int i = 1; i <= loan.TermMonths; i++)
        {
            var payment = new LoanPayment
            {
                LoanId = loan.Id,
                InstallmentNumber = i,
                Amount = loan.MonthlyPayment,
                DueDate = DateTime.UtcNow.AddMonths(i),
                IsPaid = false
            };
            await _loanRepository.AddPaymentAsync(payment);
        }

        await _loanRepository.UpdateAsync(loan);

        // Kredi tutarını hesaba yatır
        await _transactionService.MakeDepositAsync(loan.AccountId, loan.Amount);

        // Müşteriye bildirim gönder
        await _notificationRepository.AddAsync(new Notification
        {
            UserId = loan.UserId,
            Title = "Kredi Başvurunuz Onaylandı! 🎉",
            Message = $"{loan.Amount:N2} TL tutarındaki {loan.LoanType} krediniz onaylanmış ve hesabınıza yatırılmıştır.",
            Type = "Success",
            CreatedAt = DateTime.UtcNow
        });

        return true;
    }

    public async Task<bool> RejectLoanAsync(int loanId, int staffId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null || loan.Status != LoanStatus.Pending) return false;

        loan.Status = LoanStatus.Rejected;
        loan.ApprovedByStaffId = staffId;
        await _loanRepository.UpdateAsync(loan);

        // Müşteriye bildirim gönder
        await _notificationRepository.AddAsync(new Notification
        {
            UserId = loan.UserId,
            Title = "Kredi Başvurusu Hakkında Bilgilendirme",
            Message = $"{loan.Amount:N2} TL tutarındaki kredi başvurunuz maalesef onaylanmamıştır.",
            Type = "Danger",
            CreatedAt = DateTime.UtcNow
        });

        return true;
    }

    public async Task<bool> PayInstallmentAsync(int paymentId, int accountId)
    {
        var payment = await _loanRepository.GetPaymentByIdAsync(paymentId);
        if (payment == null || payment.IsPaid) return false;

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.Balance < payment.Amount) return false;

        // Hesaptan para çek (komut ile izi kalsın diye transaction service kullanılır ama withdrawal diyebiliriz)
        bool success = await _transactionService.MakeWithdrawAsync(accountId, payment.Amount);
        
        if (success) {
            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            await _loanRepository.UpdatePaymentAsync(payment);

            var loan = payment.Loan;
            loan.RemainingBalance -= payment.Amount;

            if (loan.RemainingBalance <= 0)
            {
                loan.Status = LoanStatus.Completed;
            }
            await _loanRepository.UpdateAsync(loan);
            return true;
        }

        return false;
    }

    private static LoanDto MapToDto(Loan loan) => new()
    {
        Id = loan.Id,
        UserId = loan.UserId,
        UserFullName = loan.User?.FullName ?? "",
        AccountId = loan.AccountId,
        AccountIban = loan.Account?.Iban ?? "",
        LoanType = loan.LoanType.ToString(),
        Amount = loan.Amount,
        InterestRate = loan.InterestRate,
        TermMonths = loan.TermMonths,
        MonthlyPayment = loan.MonthlyPayment,
        TotalPayment = loan.TotalPayment,
        RemainingBalance = loan.RemainingBalance,
        Status = loan.Status.ToString(),
        CreatedAt = loan.CreatedAt,
        ApprovedAt = loan.ApprovedAt,
        Payments = loan.Payments?.Select(p => new LoanPaymentDto
        {
            Id = p.Id,
            InstallmentNumber = p.InstallmentNumber,
            Amount = p.Amount,
            DueDate = p.DueDate,
            IsPaid = p.IsPaid,
            PaidAt = p.PaidAt
        }).ToList() ?? new List<LoanPaymentDto>()
    };
}
