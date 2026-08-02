using RestaurantBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantBookingSystem.Features.Notification.Services
{
    public class NotificationService : INotificationService
    {
        private readonly RestaurantReservationDbContext _context;

        public NotificationService(RestaurantReservationDbContext context)
        {
            _context = context;
        }

        public List<Models.Notification> GetAll()
        {
            return _context.Notifications.ToList();
        }

        public Models.Notification? GetById(int id)
        {
            return _context.Notifications.Find(id);
        }

        public Models.Notification Create(Models.Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            return notification;
        }

        public bool MarkAsRead(int id)
        {
            var notification = _context.Notifications.Find(id);

            if (notification == null)
                return false;

            notification.IsRead = true;

            _context.SaveChanges();

            return true;
        }

        public bool Delete(int id)
        {
            var notification = _context.Notifications.Find(id);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);

            _context.SaveChanges();

            return true;
        }
    }
}