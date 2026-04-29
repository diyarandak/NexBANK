using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;

namespace NexBank.Infrastructure.Persistence;

public class NexBankDbContext : DbContext
{
    public NexBankDbContext(DbContextOptions<NexBankDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Core.Entities.Loan> Loans { get; set; }
    public DbSet<Core.Entities.LoanPayment> LoanPayments { get; set; }
    public DbSet<Core.Entities.CreditCard> CreditCards { get; set; }
    public DbSet<Core.Entities.CreditCardTransaction> CreditCardTransactions { get; set; }
    public DbSet<Core.Entities.Bill> Bills { get; set; }
    public DbSet<Core.Entities.FavoriteRecipient> FavoriteRecipients { get; set; }
    public DbSet<Core.Entities.StandingOrder> StandingOrders { get; set; }
    public DbSet<Core.Entities.Ticket> Tickets { get; set; }
    public DbSet<Core.Entities.Notification> Notifications { get; set; }
    public DbSet<Core.Entities.Stock> Stocks { get; set; }
    public DbSet<Core.Entities.UserStock> UserStocks { get; set; }
    public DbSet<Core.Entities.SavingGoal> SavingGoals { get; set; }
    public DbSet<Core.Entities.ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TcKimlikNo);
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => e.Iban).IsUnique();
            entity.Property(e => e.AccountNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Iban).HasMaxLength(26);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.DailyLimit).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.FromAccount)
                  .WithMany(a => a.OutgoingTransactions)
                  .HasForeignKey(e => e.FromAccountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToAccount)
                  .WithMany(a => a.IncomingTransactions)
                  .HasForeignKey(e => e.ToAccountId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Loan
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(8, 5);
            entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
            entity.Property(e => e.TotalPayment).HasPrecision(18, 2);
            entity.Property(e => e.RemainingBalance).HasPrecision(18, 2);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ApprovedByStaff).WithMany().HasForeignKey(e => e.ApprovedByStaffId).OnDelete(DeleteBehavior.SetNull);
        });

        // LoanPayment
        modelBuilder.Entity<LoanPayment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Loan).WithMany(l => l.Payments).HasForeignKey(e => e.LoanId).OnDelete(DeleteBehavior.Cascade);
        });

        // CreditCard
        modelBuilder.Entity<CreditCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CardNumber).IsUnique();
            entity.Property(e => e.CardNumber).HasMaxLength(16);
            entity.Property(e => e.Cvv).HasMaxLength(3);
            entity.Property(e => e.ExpiryDate).HasMaxLength(5);
            entity.Property(e => e.CardLimit).HasPrecision(18, 2);
            entity.Property(e => e.UsedAmount).HasPrecision(18, 2);
            entity.Ignore(e => e.AvailableLimit);
            entity.Ignore(e => e.MinPayment);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // CreditCardTransaction
        modelBuilder.Entity<CreditCardTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);

            entity.HasOne(e => e.CreditCard).WithMany(cc => cc.Transactions).HasForeignKey(e => e.CreditCardId).OnDelete(DeleteBehavior.Cascade);
        });

        // FavoriteRecipient
        modelBuilder.Entity<FavoriteRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecipientIban).HasMaxLength(26).IsRequired();
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.Label).HasMaxLength(50);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Bill
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.InstitutionName).HasMaxLength(100);
            entity.Property(e => e.SubscriberNo).HasMaxLength(30);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PaidFromAccount).WithMany().HasForeignKey(e => e.PaidFromAccountId).OnDelete(DeleteBehavior.SetNull);
        });

        // StandingOrder
        modelBuilder.Entity<StandingOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ToIban).HasMaxLength(26);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Frequency).HasMaxLength(20);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FromAccount).WithMany().HasForeignKey(e => e.FromAccountId).OnDelete(DeleteBehavior.Cascade);
        });

        // SavingGoal
        modelBuilder.Entity<SavingGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetAmount).HasPrecision(18, 2);
            entity.Property(e => e.CurrentAmount).HasPrecision(18, 2);
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage — Canlı Destek Mesajları
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Branch).HasMaxLength(100);

            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Receiver)
                  .WithMany()
                  .HasForeignKey(e => e.ReceiverId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
