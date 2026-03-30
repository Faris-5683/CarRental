using CarRental.Business.DTOs.Admin;
using CarRental.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]        // ← entire controller is admin only
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ── User Management ──────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null) return NotFound("User not found.");
        return Ok(user);
    }

    [HttpPut("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        await _adminService.DeactivateUserAsync(id);
        return NoContent();
    }

    [HttpPut("users/{id:int}/reactivate")]
    public async Task<IActionResult> ReactivateUser(int id)
    {
        await _adminService.ReactivateUserAsync(id);
        return NoContent();
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _adminService.DeleteUserAsync(id);
        return NoContent();
    }

    // ── Car Management ────────────────────────────────────

    [HttpGet("cars")]
    public async Task<IActionResult> GetAllCars()
    {
        var cars = await _adminService.GetAllCarsAsync();
        return Ok(cars);
    }

    [HttpDelete("cars/{id:int}")]
    public async Task<IActionResult> ForceDeleteCar(int id)
    {
        await _adminService.ForceDeleteCarAsync(id);
        return NoContent();
    }

    // ── Admin Management ──────────────────────────────────

    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
    {
        var result = await _adminService.CreateAdminAsync(dto);
        return Ok(result);
    }

    [HttpDelete("admins/{id:int}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var callerAdminId = GetUserId();
        await _adminService.DeleteAdminAsync(callerAdminId, id);
        return NoContent();
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}