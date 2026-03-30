using CarRental.DataAccess.Context;
using CarRental.DataAccess.Interfaces;
using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    // Soft delete — regular user deletes their own account
    public async Task SoftDeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdIgnoreFilterAsync(int id)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task DeactivateAsync(int id)
    {
        var user = await GetByIdIgnoreFilterAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReactivateAsync(int id)
    {
        var user = await GetByIdIgnoreFilterAsync(id);
        if (user != null)
        {
            user.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }

    // Hard delete — admin permanently removes a user
    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}