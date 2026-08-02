using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Notification.Repositories;

public interface INotificationRepository
{
    Task<List<Models.Notification>> GetByUserIdAsync(int userId);
    Task<Models.Notification?> GetByIdAsync(int id);
    Task<bool> UserExistsAsync(int userId);
    Task AddAsync(Models.Notification notification);
    Task SaveChangesAsync();
}