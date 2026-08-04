using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Reservation.DTOs;
using RestaurantBookingSystem.Features.Reservation.Services;

namespace RestaurantBookingSystem.Features.Reservation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            var reservation = await _reservationService.CreateReservationAsync(dto, userId);
            return CreatedAtAction(nameof(GetReservationsByCustomer), new { userId }, reservation);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyReservations()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        return Ok(await _reservationService.GetReservationsByCustomerAsync(userId));
    }

    [HttpGet("customer/{userId:int}")]
    public async Task<IActionResult> GetReservationsByCustomer(int userId)
    {
        if (!TryGetUserId(out var currentUserId))
            return Unauthorized();
        if (currentUserId != userId && !User.IsInRole(RoleNames.Admin))
            return Forbid();
        return Ok(await _reservationService.GetReservationsByCustomerAsync(userId));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetReservationsByDate(DateOnly date) =>
        Ok(await _reservationService.GetReservationsByDateAsync(date));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            return Ok(await _reservationService.UpdateReservationAsync(id, dto, userId, User.IsInRole(RoleNames.Admin)));
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        try
        {
            await _reservationService.CancelReservationAsync(id, userId, User.IsInRole(RoleNames.Admin));
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
