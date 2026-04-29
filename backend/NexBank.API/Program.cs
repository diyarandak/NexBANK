using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexBank.Application.Interfaces;
using NexBank.Application.Patterns.Factory;
using NexBank.Application.Patterns.Strategy;
using NexBank.Application.Patterns.Observer;
using NexBank.Application.Patterns.Command;
using NexBank.Application.Services;
using NexBank.Core.Interfaces;
using NexBank.Infrastructure.Persistence;
using NexBank.API.Hubs;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using NexBank.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<MarketUpdateWorker>();

// ── DATABASE ── (SQLite — Kalıcı veritabanı)
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "nexbank.db");
builder.Services.AddDbContext<NexBankDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// ── JWT AUTHENTICATION ──
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "NexBankSuperSecretKey2024!@#$%^&*()VeryLong";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NexBank",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NexBankApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };

        // ── SignalR WebSocket desteği ──
        // WebSocket bağlantılarında Authorization header gönderilemez,
        // token query string üzerinden gelir. Bu olmadan ChatHub çalışmaz.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/marketHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ── CORS ── (Vue frontend'e izin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── DEPENDENCY INJECTION ──
// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IFavoriteRecipientRepository, FavoriteRecipientRepository>();
builder.Services.AddScoped<IStandingOrderRepository, StandingOrderRepository>();
builder.Services.AddScoped<ISupportRepository, SupportRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Decorator Pattern for Caching
builder.Services.AddScoped<StockRepository>(); // Ham repository
builder.Services.AddScoped<IStockRepository>(provider => 
{
    var inner = provider.GetRequiredService<StockRepository>();
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new CachedStockRepository(inner, cache);
});

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddMediatR(typeof(AccountService).Assembly);

// Factory Pattern
builder.Services.AddSingleton<IAccountFactory, AccountFactory>();

// Strategy Pattern
builder.Services.AddScoped<IPaymentStrategy, HavaleStrategy>();
builder.Services.AddScoped<IPaymentStrategy, EftStrategy>();
builder.Services.AddScoped<IPaymentStrategy, SwiftStrategy>();
builder.Services.AddScoped<IPaymentStrategy, QrTransferStrategy>();
builder.Services.AddScoped<PaymentStrategyContext>();

// Observer Pattern
builder.Services.AddScoped<ITransactionObserver, SmsNotificationObserver>();
builder.Services.AddScoped<ITransactionObserver, EmailNotificationObserver>();
builder.Services.AddScoped<ITransactionObserver, TransactionLogObserver>();
builder.Services.AddScoped<ITransactionObserver, FraudDetectionObserver>();
builder.Services.AddScoped<TransactionSubject>(provider => 
{
    var subject = new TransactionSubject();
    var observers = provider.GetServices<ITransactionObserver>();
    foreach(var obs in observers) 
    {
        subject.Attach(obs);
    }
    return subject;
});

// Command Pattern
builder.Services.AddScoped<TransactionInvoker>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
// Yeni eklenen servisler
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ICreditCardService, CreditCardService>();
builder.Services.AddScoped<IBillService, UtilityService>();
builder.Services.AddScoped<IFavoriteRecipientService, UtilityService>();
builder.Services.AddScoped<IStandingOrderService, UtilityService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── SEED DATA ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NexBankDbContext>();
    context.Database.EnsureCreated(); // SQLite tabloları yoksa oluştur
    await DbSeeder.SeedAsync(context);
}

app.UseRouting();

// ── CORS (En başta olmalı) ──
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MarketHub>("/marketHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();
