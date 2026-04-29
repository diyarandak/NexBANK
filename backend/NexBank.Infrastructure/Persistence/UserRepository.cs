using Microsoft.EntityFrameworkCore;
using NexBank.Core.Entities;
using NexBank.Core.Interfaces;

namespace NexBank.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly NexBankDbContext _context;

    public UserRepository(NexBankDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByTcKimlikAsync(string tcKimlikNo) =>
        await _context.Users.FirstOrDefaultAsync(u => u.TcKimlikNo == tcKimlikNo);

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.Include(u => u.Accounts).ToListAsync();

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync() =>
        await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
}
