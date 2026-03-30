// DataAccess/Seeding/DbSeeder.cs
using CarRental.DataAccess.Context;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CarRental.DataAccess.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        // IgnoreQueryFilters ensures we check ALL users including inactive ones
        if (context.Users.IgnoreQueryFilters().Any(u => u.Role == UserRole.Admin))
            return;

        var email = configuration["AdminSettings:Email"]!;
        var password = configuration["AdminSettings:Password"]!;
        var firstName = configuration["AdminSettings:FirstName"]!;
        var lastName = configuration["AdminSettings:LastName"]!;
        var phoneNumber = configuration["AdminSettings:PhoneNumber"]!;

        var admin = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}