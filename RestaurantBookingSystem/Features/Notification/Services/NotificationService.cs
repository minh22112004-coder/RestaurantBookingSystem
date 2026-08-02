using RestaurantBookingSystem.Features.Notification.DTOs;
using RestaurantBookingSystem.Features.Notification.Repositories;

namespace RestaurantBookingSystem.Features.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationResponse>> GetByUserIdAsync(int userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<NotificationResponse?> GetByIdAsync(int id)
    {
        var notification = await _repository.GetByIdAsync(id);
        return notification is null ? null : MapToResponse(notification);
    }

    public async Task<NotificationResponse?> CreateAsync(CreateNotificationRequest request)
    {
        if (!await _repository.UserExistsAsync(request.UserId))
            return null;

        var notification = new Models.Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();
        return MapToResponse(notification);
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var notification = await _repository.GetByIdAsync(id);
        if (notification is null)
            return false;

        if (notification.IsRead == true)
            return true;

        notification.IsRead = true;
        await _repository.SaveChangesAsync();
        return true;
    }

    private static NotificationResponse MapToResponse(Models.Notification notification)
    {
        return new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead ?? false,
            CreatedAt = notification.CreatedAt ?? DateTime.MinValue
        };
    }
}