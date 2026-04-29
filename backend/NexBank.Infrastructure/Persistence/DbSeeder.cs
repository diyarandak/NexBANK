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
        Console.WriteLine("[SEED] Demo kullanıcı (12345678910) SİLİNDİ VE YENİDEN OLUŞTURULDU!");

        // 3. HESAP KONTROLÜ
        var demoAccount = new Account
        {
            UserId = demoCustomer.Id,
            AccountNumber = "NB-12345678",
            Iban = "TR92 0006 1000 1234 5678 9012 34",
            Balance = 84250.75m,
            Currency = "TRY",
            AccountType = AccountType.Individual,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Accounts.Add(demoAccount);
        await context.SaveChangesAsync();
        Console.WriteLine("[SEED] Demo hesap (84.250 TL) OLUŞTURULDU.");

        Console.WriteLine(">>>> VERİTABANI KONTROLÜ TAMAMLANDI <<<<");
        Console.WriteLine("****************************************************\n");
    }
}
