// Business/Interfaces/IAdminService.cs
using CarRental.Business.DTOs.Admin;
using CarRental.Business.DTOs.Auth;

namespace CarRental.Business.Interfaces;

public interface IAdminService
{
    // User management
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(int id);
    Task DeactivateUserAsync(int id);
    Task ReactivateUserAsync(int id);
    Task DeleteUserAsync(int id);

    // Car management
    Task<IEnumerable<AdminCarDto>> GetAllCarsAsync();
    Task ForceDeleteCarAsync(int id);

    // Admin management
    Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto dto);
    Task DeleteAdminAsync(int callerAdminId, int targetId);
}