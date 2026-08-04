using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;
using RestaurantBookingSystem.Web.ViewComponents;

namespace RestaurantBookingSystem.Web.Controllers;

[Route("notifications")]
[RequireSessionRole("Customer")]
public sealed class NotificationViewController : Controller
{
    private readonly INotificationApiClient _notificationApiClient;
    private readonly IJwtSessionService _sessionService;
    private readonly ILogger<NotificationViewController> _logger;

    public NotificationViewController(
        INotificationApiClient notificationApiClient,
        IJwtSessionService sessionService,
        ILogger<NotificationViewController> logger)
    {
        _notificationApiClient = notificationApiClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var notifications = await _notificationApiClient.GetByUserAsync(
                _sessionService.Current!.UserId,
                cancellationToken);
            var ordered = notifications.OrderByDescending(notification => notification.CreatedAt).ToList();
            HttpContext.Items[NotificationBadgeViewComponent.CacheKey] = ordered;
            return View("~/Views/Notification/Index.cshtml", new NotificationListViewModel { Notifications = ordered });
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load customer notifications.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("~/Views/Notification/Index.cshtml", new NotificationListViewModel
            {
                ErrorMessage = "Notifications are temporarily unavailable. Please try again shortly."
            });
        }
    }

    [HttpPost("{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationApiClient.MarkAsReadAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Notification marked as read.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException exception) when (
            exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            _logger.LogWarning(exception, "Notification {NotificationId} could not be marked as read.", id);
            Response.StatusCode = exception.StatusCode == HttpStatusCode.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status403Forbidden;
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                StatusCode = Response.StatusCode,
                Title = "Notification unavailable",
                Message = "The notification could not be found or does not belong to your account."
            });
        }
    }
}
