using CarRental.DataAccess.Context;
using CarRental.DataAccess.Interfaces;
using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.DataAccess.Repositories;

public class CarRepository : ICarRepository
{
    private readonly AppDbContext _context;

    public CarRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Car>> GetAllAsync()
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.Images)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Cars
            .Include(c => c.Images)
            .Include(c => c.Owner)
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        return await _context.Cars
            .Include(c => c.Owner)
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Car car)
    {
        await _context.Cars.AddAsync(car);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Car car)
    {
        _context.Cars.Update(car);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// It does a soft delete by setting IsActive = false instead of removing the record — this protects existing booking history.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAsync(int id)
    {
        var car = await GetByIdAsync(id);
        if (car != null)
        {
            car.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<IEnumerable<Car>> GetAllIgnoreFilterAsync()
    {
        return await _context.Cars
            .IgnoreQueryFilters()
            .Include(c => c.Owner)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task ForceDeleteAsync(int id)
    {
        var car = await _context.Cars
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car != null)
        {
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }
    }
}