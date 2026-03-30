// Business/Services/AuthService.cs
using AutoMapper;
using CarRental.Business.DTOs.Auth;
using CarRental.Business.Interfaces;
using CarRental.DataAccess.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IMapper mapper,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if email already taken
        if (await _userRepository.ExistsAsync(dto.Email))
            throw new InvalidOperationException("Email is already registered.");

        // Create user with hashed password
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Map and attach token
        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _jwtService.GenerateToken(user);
        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Always show same message — don't reveal if email exists or not
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Your account has been deactivated. Please contact support.");

        // Map and attach token
        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _jwtService.GenerateToken(user);
        return response;
    }
}