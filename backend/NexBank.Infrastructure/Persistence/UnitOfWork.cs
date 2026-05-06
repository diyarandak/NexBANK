using Microsoft.EntityFrameworkCore.Storage;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly NexBankDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(NexBankDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            
            // SINGLETON PATTERN: Kayıt istatistiğini güncelle
            NexBank.Application.Patterns.Singleton.DatabaseManager.Instance.IncrementSaveCount();
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        var result = await _context.SaveChangesAsync();
        
        // SINGLETON PATTERN: Kayıt istatistiğini güncelle
        NexBank.Application.Patterns.Singleton.DatabaseManager.Instance.IncrementSaveCount();
        
        return result;
    }

    public void Dispose()
    {
        if (_currentTransaction != null)
        {
            _currentTransaction.Dispose();
        }
        _context.Dispose();
    }
}
