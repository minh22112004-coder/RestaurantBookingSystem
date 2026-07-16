using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Notification.Services
{
    public interface INotificationService
    {
        List<Models.Notification> GetAll();

        Models.Notification? GetById(int id);

        Models.Notification Create(Models.Notification notification);

        bool MarkAsRead(int id);

        bool Delete(int id);
    }
}
