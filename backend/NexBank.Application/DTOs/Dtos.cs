namespace NexBank.Application.DTOs;

// ── Auth DTOs ──

// Müşteri girişi: TC Kimlik No + Şifre
public class CustomerLoginRequestDto
{
    public string TcKimlikNo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Personel girişi: E-posta + Şifre
public class StaffLoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

// Müşteri kaydı
public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Branch { get; set; } = "Yalova Merkez"; // Kayıt sırasında şube seçimi
}

// Personel kaydı (admin tarafından oluşturulur)
public class StaffRegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Branch { get; set; } = "Merkez Şube";
}

// ── User DTOs ──
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TcKimlikNo { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

// ── Account DTOs ──
public class AccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal DailyLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
}

public class CreateAccountDto
{
    public string AccountType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Currency { get; set; } = "TRY";
}

// ── Transaction DTOs ──
public class TransactionDto
{
    public int Id { get; set; }
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

// ── Dashboard DTO ──
public class DashboardDto
{
    public decimal TotalBalance { get; set; }
    public int ActiveAccounts { get; set; }
    public decimal TodayVolume { get; set; }
    public int PendingTransactions { get; set; }
    public decimal CreditCardDebt { get; set; }
    public decimal ActiveLoansTotal { get; set; }
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

// ── Transfer DTO (IBAN destekli) ──
public class TransferRequestDto
{
    public int FromAccountId { get; set; }
    public string ToIban { get; set; } = string.Empty; // IBAN ile transfer
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Havale";
}

// ── Para Yatırma / Çekme ──
public class DepositWithdrawDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
}

// ── Kredi DTOs ──
public class LoanApplicationDto
{
    public int AccountId { get; set; }
    public string LoanType { get; set; } = string.Empty; // Ihtiyac, Konut, Tasit
    public decimal Amount { get; set; }
    public int TermMonths { get; set; } // 6, 12, 24, 36, 48, 60
}

public class LoanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountIban { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public List<LoanPaymentDto> Payments { get; set; } = new();
}

public class LoanPaymentDto
{
    public int Id { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}

// ── Kredi Kartı DTOs ──
public class CreditCardDto
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public decimal CardLimit { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal AvailableLimit { get; set; }
    public decimal MinPayment { get; set; }
    public bool IsActive { get; set; }
    public List<CreditCardTransactionDto> Transactions { get; set; } = new();
}

public class CreditCardTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreditCardSpendDto
{
    public int CreditCardId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Alışveriş";
}

public class CashAdvanceDto
{
    public int CreditCardId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public int Installments { get; set; } // Taksit sayısı eklendi
}

public class CreditCardPaymentDto
{
    public int CreditCardId { get; set; }
    public int FromAccountId { get; set; }
    public decimal Amount { get; set; }
}

// ── Favori Alıcı DTOs ──
public class FavoriteRecipientDto
{
    public int Id { get; set; }
    public string RecipientIban { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class CreateFavoriteDto
{
    public string RecipientIban { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// ── Fatura DTOs ──
public class BillDto
{
    public int Id { get; set; }
    public string BillType { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string SubscriberNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreateBillDto
{
    public string BillType { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string SubscriberNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}

public class PayBillDto
{
    public int BillId { get; set; }
    public int AccountId { get; set; }
}

// ── Otomatik Ödeme DTOs ──
public class StandingOrderDto
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public string FromAccountIban { get; set; } = string.Empty;
    public string ToIban { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime NextPaymentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStandingOrderDto
{
    public int FromAccountId { get; set; }
    public string ToIban { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "Aylık";
    public string Description { get; set; } = string.Empty;
    public DateTime NextPaymentDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ── Harcama Analizi DTO ──
public class SpendingAnalyticsDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public List<CategorySpendingDto> ByCategory { get; set; } = new();
    public List<MonthlySpendingDto> MonthlyTrend { get; set; } = new();
}

public class CategorySpendingDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class MonthlySpendingDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}
// ── Ticket DTOs ──
public class CreateTicketDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
}
