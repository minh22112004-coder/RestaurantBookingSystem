using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Notification.DTOs;
using RestaurantBookingSystem.Features.Notification.Services;

namespace RestaurantBookingSystem.Features.Notification.Controllers;

[ApiController]
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
        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        return notification is null ? NotFound() : Ok(notification);
    }

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
        var success = await _notificationService.MarkAsReadAsync(id);
        return success ? NoContent() : NotFound();
    }
}
