using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ViewComponents;

public sealed class NotificationBadgeViewComponent : ViewComponent
{
    public const string CacheKey = "CustomerNotifications";

    private readonly IJwtSessionService _sessionService;
    private readonly INotificationApiClient _notificationApiClient;
    private readonly ILogger<NotificationBadgeViewComponent> _logger;

    public NotificationBadgeViewComponent(
        IJwtSessionService sessionService,
        INotificationApiClient notificationApiClient,
        ILogger<NotificationBadgeViewComponent> logger)
    {
        _sessionService = sessionService;
        _notificationApiClient = notificationApiClient;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var currentUser = _sessionService.Current;
        if (currentUser?.IsCustomer != true)
            return Content(string.Empty);

        try
        {
            List<NotificationDto> notifications;
            if (HttpContext.Items.TryGetValue(CacheKey, out var cached) && cached is List<NotificationDto> cachedNotifications)
            {
                notifications = cachedNotifications;
            }
            else
            {
                notifications = await _notificationApiClient.GetByUserAsync(currentUser.UserId);
                HttpContext.Items[CacheKey] = notifications;
            }

            return View(notifications.Count(notification => !notification.IsRead));
        }
        catch (ApiClientException exception)
        {
            _logger.LogDebug(exception, "Unable to load the notification badge.");
            return Content(string.Empty);
        }
    }
}
