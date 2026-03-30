using CarRental.Domain.Enums;

namespace CarRental.Business.DTOs.Admin;

public class AdminCarDto
{
    public int Id { get; set; }
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = default!;
    public decimal PricePerDay { get; set; }
    public string City { get; set; } = default!;
    public CarStatus Status { get; set; }
    public bool IsActive { get; set; }
    public string OwnerName { get; set; } = default!;
    public string OwnerEmail { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}