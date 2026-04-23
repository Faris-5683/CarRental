// Business/Services/BookingService.cs
using AutoMapper;
using CarRental.Business.DTOs.Booking;
using CarRental.Business.Interfaces;
using CarRental.DataAccess.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.Business.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;

    public BookingService(
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int renterId)
    {
        var bookings = await _bookingRepository.GetByRenterIdAsync(renterId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByCarAsync(int carId, int ownerId)
    {
        var car = await _carRepository.GetByIdAsync(carId);

        if (car == null)
            throw new KeyNotFoundException("Car not found.");

        if (car.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You are not the owner of this car.");

        var bookings = await _bookingRepository.GetByCarIdAsync(carId);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        return booking == null ? null : _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(int renterId, CreateBookingDto dto)
    {
        var car = await _carRepository.GetByIdAsync(dto.CarId);

        if (car == null)
            throw new KeyNotFoundException("Car not found.");

        // Owner cannot book their own car
        if (car.OwnerId == renterId)
            throw new InvalidOperationException("You cannot book your own car.");

        if (car.Status != CarStatus.Available)
            throw new InvalidOperationException("Car is not available for booking.");

        if (dto.StartDate >= dto.EndDate)
            throw new InvalidOperationException("End date must be after start date.");

        if (dto.StartDate < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Start date cannot be in the past.");

        // Calculate total price
        var days = (dto.EndDate - dto.StartDate).Days;
        var totalPrice = days * car.PricePerDay;

        var booking = new Booking
        {
            CarId = dto.CarId,
            RenterId = renterId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        // Update car status
        car.Status = CarStatus.Booked;
        await _carRepository.UpdateAsync(car);

        await _bookingRepository.AddAsync(booking);

        var created = await _bookingRepository.GetByIdAsync(booking.Id);
        return _mapper.Map<BookingDto>(created!);
    }

    public async Task CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        // Only renter or car owner can cancel
        if (booking.RenterId != userId && booking.Car.OwnerId != userId)
            throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");

        if (booking.Status == BookingStatus.Completed)
            throw new InvalidOperationException("Completed bookings cannot be cancelled.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        booking.Status = BookingStatus.Cancelled;

        // Free up the car
        booking.Car.Status = CarStatus.Available;
        await _carRepository.UpdateAsync(booking.Car);

        await _bookingRepository.UpdateAsync(booking);
    }
}