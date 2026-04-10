using CarRental.API.Middleware;
using CarRental.Business.Interfaces;
using CarRental.Business.Mappings;
using CarRental.Business.Services;
using CarRental.DataAccess.Caching;
using CarRental.DataAccess.Context;
using CarRental.DataAccess.Interfaces;
using CarRental.DataAccess.Repositories;
using CarRental.DataAccess.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    ));

// ── Redis ─────────────────────────────────────────────────
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.InstanceName = "CarRental_";
        options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnection);
        options.ConfigurationOptions.AbortOnConnectFail = false;
        options.ConfigurationOptions.ConnectTimeout = 3000;
        options.ConfigurationOptions.SyncTimeout = 3000;
        options.ConfigurationOptions.ConnectRetry = 2;
    });
}
else
{
    // Fallback to in-memory cache if Redis is not configured
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// ── AutoMapper ────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AuthMappingProfile>();
    cfg.AddProfile<CarMappingProfile>();
    cfg.AddProfile<BookingMappingProfile>();
    cfg.AddProfile<AdminMappingProfile>();
});

// ── JWT Authentication ────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

// ── Dependency Injection ──────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICarImageRepository, CarImageRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// ── Controllers + JSON ────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── Swagger ───────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token here"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Database Seeding ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var retryCount = 0;
    const int maxRetries = 5;

    while (retryCount < maxRetries)
    {
        try
        {
            logger.LogInformation("Attempting database seeding... Attempt {Attempt}", retryCount + 1);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            await DbSeeder.SeedAsync(context, configuration);
            logger.LogInformation("Database seeding completed successfully.");
            break; // success — exit the loop
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogError(ex, "Seeding attempt {Attempt} failed: {Message}", retryCount, ex.Message);

            if (retryCount >= maxRetries)
            {
                logger.LogError("All seeding attempts failed. App will continue without seeding.");
                break; // do not crash — just continue
            }

            logger.LogInformation("Retrying in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

// ── Middleware Pipeline ───────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();   ← commented out for Azure Linux

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();