namespace NexBank.Core.Entities;

public enum AccountType
{
    Individual,   // Bireysel
    Corporate,    // Kurumsal
    Savings,      // Tasarruf
    Deposit       // Vadeli
}

public enum TransactionType
{
    Transfer,     // Havale/EFT
    Deposit,      // Para Yatırma
    Withdrawal    // Para Çekme
}

public enum TransactionStatus
{
    Pending,      // Beklemede
    Approved,     // Onaylandı
    Rejected      // Reddedildi
}

public enum PaymentMethod
{
    Havale,
    EFT,
    SWIFT,
    QRTransfer
}

public enum UserRole
{
    Admin,
    Customer
}

public enum LoanType
{
    Ihtiyac,      // İhtiyaç Kredisi
    Konut,        // Konut Kredisi
    Tasit         // Taşıt Kredisi
}

public enum LoanStatus
{
    Pending,      // Beklemede
    Approved,     // Onaylandı
    Rejected,     // Reddedildi
    Completed     // Tamamlandı (tüm taksitler ödendi)
}

public enum BillType
{
    Elektrik,
    Su,
    Dogalgaz,
    Internet,
    Telefon
}
