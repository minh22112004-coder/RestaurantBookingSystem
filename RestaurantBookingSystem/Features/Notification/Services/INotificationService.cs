using RestaurantBookingSystem.Features.Notification.DTOs;

namespace RestaurantBookingSystem.Features.Notification.Services;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetByUserIdAsync(int userId);
    Task<NotificationResponse?> GetByIdAsync(int id);
    Task<NotificationResponse?> CreateAsync(CreateNotificationRequest request);
    Task<bool> MarkAsReadAsync(int id);
}