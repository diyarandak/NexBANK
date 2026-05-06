using Microsoft.EntityFrameworkCore;
using NexBank.Application.Services;
using NexBank.Core.Entities;

namespace NexBank.Infrastructure.Persistence;

/// <summary>
/// Veritabanı başlatıcı — İlk çalıştırmada banka personellerini oluşturur.
/// Müşteriler kayıt ekranından kendi hesaplarını açar.
/// Personeller ise banka tarafından sisteme önceden tanımlanmıştır.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(NexBankDbContext context)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try 
        {
            Console.WriteLine("\n****************************************************");
            Console.WriteLine(">>>> NEXBANK VERİTABANI KONTROLÜ BAŞLATILDI <<<<");

            // 1. PERSONEL KONTROLÜ (Diyar Andak)
            var mainStaff = await context.Users.FirstOrDefaultAsync(u => u.Email == "diyarandak@nexbank.com");
            if (mainStaff == null)
            {
                mainStaff = new User
                {
                    FullName = "Diyar Andak",
                    Email = "diyarandak@nexbank.com",
                    TcKimlikNo = "10039962842",
                    PasswordHash = AuthService.HashPassword("123Diyar321"),
                    Role = UserRole.Admin,
                    Branch = "Yalova Şubesi",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                context.Users.Add(mainStaff);
                Console.WriteLine("[SEED] Personel hesabı (diyarandak@nexbank.com) OLUŞTURULDU.");
            }
            else {
                mainStaff.PasswordHash = AuthService.HashPassword("123Diyar321");
                mainStaff.Role = UserRole.Admin;
                Console.WriteLine("[SEED] Personel hesabı GÜNCELLENDİ.");
            }
            await context.SaveChangesAsync();

            // 2. DEMO MÜŞTERİ KONTROLÜ (12345678910) - ZORLA YENİLEME
            var existingDemo = await context.Users.FirstOrDefaultAsync(u => u.TcKimlikNo == "12345678910");
            if (existingDemo != null)
            {
                context.Users.Remove(existingDemo);
                await context.SaveChangesAsync();
                Console.WriteLine("[SEED] Eski demo kullanıcı temizlendi...");
            }

            var demoCustomer = new User
            {
                FullName = "Diyar Andak",
                Email = "nexbank@gmail.com",
                TcKimlikNo = "12345678910",
                PasswordHash = AuthService.HashPassword("123Diyar321"),
                Role = UserRole.Customer,
                Branch = "Yalova Şubesi",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Users.Add(demoCustomer);
            await context.SaveChangesAsync();

            // 3. HESAP KONTROLÜ
            var demoAccount = new Account
            {
                UserId = demoCustomer.Id,
                AccountNumber = "NB-12345678",
                Iban = "TR92 0006 1000 1234 5678 9012 34",
                Balance = 84250.75m,
                Currency = "TRY",
                DailyLimit = 100000m, // Günlük limit eklendi
                AccountType = AccountType.Individual,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Accounts.Add(demoAccount);
            await context.SaveChangesAsync();

            // 4. ALİ CAN HESABI (Transfer Testi İçin)
            var aliCan = await context.Users.FirstOrDefaultAsync(u => u.FullName == "Ali Can");
            if (aliCan == null)
            {
                aliCan = new User
                {
                    FullName = "ali can",
                    Email = "alican@example.com",
                    TcKimlikNo = "99999999999",
                    PasswordHash = AuthService.HashPassword("123Ali123"),
                    Role = UserRole.Customer,
                    Branch = "İstanbul Şubesi",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                context.Users.Add(aliCan);
                await context.SaveChangesAsync();

                var aliAccount = new Account
                {
                    UserId = aliCan.Id,
                    AccountNumber = "NB-99998888",
                    Iban = "TR4500012743070059226516", // Kullanıcının istediği IBAN
                    Balance = 500.00m,
                    Currency = "TRY",
                    DailyLimit = 100000m,
                    AccountType = AccountType.Individual,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                context.Accounts.Add(aliAccount);
                await context.SaveChangesAsync();
                Console.WriteLine("[SEED] Ali Can hesabı OLUŞTURULDU (IBAN: TR4500012743070059226516).");
            }

            await transaction.CommitAsync();
            Console.WriteLine("[SEED] Veriler başarıyla işlendi.");
            Console.WriteLine(">>>> VERİTABANI KONTROLÜ TAMAMLANDI <<<<");
            Console.WriteLine("****************************************************\n");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"[SEED ERROR] Hata oluştu: {ex.Message}");
        }
    }
}
