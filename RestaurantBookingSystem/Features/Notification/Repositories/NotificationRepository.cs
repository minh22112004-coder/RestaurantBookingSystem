using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Notification.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly RestaurantReservationDbContext _context;

    public NotificationRepository(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Models.Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync();
    }

    public async Task<Models.Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(user => user.UserId == userId);
    }

    public async Task AddAsync(Models.Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
