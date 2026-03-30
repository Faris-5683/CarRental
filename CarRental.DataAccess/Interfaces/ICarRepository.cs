using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.DataAccess.Interfaces
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllAsync();
        Task<IEnumerable<Car>> GetByOwnerIdAsync(int ownerId);
        Task<Car?> GetByIdAsync(int id);
        Task AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(int id);

        // Admin specific
        Task<IEnumerable<Car>> GetAllIgnoreFilterAsync();
        Task ForceDeleteAsync(int id);
    }
}
