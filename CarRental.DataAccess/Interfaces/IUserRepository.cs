namespace CarRental.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);

    // Admin specific
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdIgnoreFilterAsync(int id);
    Task DeactivateAsync(int id);
    Task ReactivateAsync(int id);
    Task SoftDeleteAsync(int id);    // user deletes own account
    Task DeleteAsync(int id);        // admin hard deletes
}