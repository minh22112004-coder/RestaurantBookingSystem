using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Notification.DTOs;
using RestaurantBookingSystem.Features.Notification.Services;

namespace RestaurantBookingSystem.Features.Notification.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        if (!CanAccessUser(userId))
            return Forbid();
        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        if (notification is null)
            return NotFound();
        return CanAccessUser(notification.UserId ?? 0) ? Ok(notification) : Forbid();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var notification = await _notificationService.CreateAsync(request);
        if (notification is null)
            return NotFound(new { message = $"User with ID {request.UserId} was not found." });

        return CreatedAtAction(
            nameof(GetById),
            new { id = notification.NotificationId },
            notification);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        if (notification is null)
            return NotFound();
        if (!CanAccessUser(notification.UserId ?? 0))
            return Forbid();
        var success = await _notificationService.MarkAsReadAsync(id);
        return success ? NoContent() : NotFound();
    }

    private bool CanAccessUser(int userId)
    {
        return User.IsInRole(RoleNames.Admin) ||
               int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) &&
               currentUserId == userId;
    }
}
