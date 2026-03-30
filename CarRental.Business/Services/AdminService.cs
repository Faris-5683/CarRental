// Business/Services/AdminService.cs
using AutoMapper;
using CarRental.Business.DTOs.Admin;
using CarRental.Business.DTOs.Auth;
using CarRental.Business.Interfaces;
using CarRental.DataAccess.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.Business.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public AdminService(
        IUserRepository userRepository,
        ICarRepository carRepository,
        IMapper mapper,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _carRepository = carRepository;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    // ── User Management ──────────────────────────────────

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AdminUserDto>>(users);
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdIgnoreFilterAsync(id);
        return user == null ? null : _mapper.Map<AdminUserDto>(user);
    }

    public async Task DeactivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdIgnoreFilterAsync(id);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin accounts cannot be deactivated.");

        if (!user.IsActive)
            throw new InvalidOperationException("User is already deactivated.");

        await _userRepository.DeactivateAsync(id);
    }

    public async Task ReactivateUserAsync(int id)
    {
        var user = await _userRepository.GetByIdIgnoreFilterAsync(id);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.IsActive)
            throw new InvalidOperationException("User is already active.");

        await _userRepository.ReactivateAsync(id);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdIgnoreFilterAsync(id);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin accounts cannot be deleted from here.");

        await _userRepository.DeleteAsync(id);
    }

    // ── Car Management ────────────────────────────────────

    public async Task<IEnumerable<AdminCarDto>> GetAllCarsAsync()
    {
        var cars = await _carRepository.GetAllIgnoreFilterAsync();
        return _mapper.Map<IEnumerable<AdminCarDto>>(cars);
    }

    public async Task ForceDeleteCarAsync(int id)
    {
        var car = await _carRepository.GetByIdAsync(id);

        if (car == null)
            throw new KeyNotFoundException("Car not found.");

        await _carRepository.ForceDeleteAsync(id);
    }

    // ── Admin Management ──────────────────────────────────

    public async Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto dto)
    {
        if (await _userRepository.ExistsAsync(dto.Email))
            throw new InvalidOperationException("Email is already registered.");

        var admin = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(admin);

        var response = _mapper.Map<AuthResponseDto>(admin);
        response.Token = _jwtService.GenerateToken(admin);
        return response;
    }

    public async Task DeleteAdminAsync(int callerAdminId, int targetId)
    {
        // Prevent admin from deleting themselves
        if (callerAdminId == targetId)
            throw new InvalidOperationException("You cannot delete your own admin account.");

        var admin = await _userRepository.GetByIdIgnoreFilterAsync(targetId);

        if (admin == null)
            throw new KeyNotFoundException("Admin not found.");

        if (admin.Role != UserRole.Admin)
            throw new InvalidOperationException("User is not an admin.");

        await _userRepository.DeleteAsync(targetId);
    }
}