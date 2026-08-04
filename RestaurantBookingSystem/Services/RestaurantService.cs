using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.Restaurant;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Services;

public class RestaurantService : IRestaurantService
{
    private readonly RestaurantReservationDbContext _context;

    public RestaurantService(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RestaurantResponseDto>> GetAllAsync()
    {
        var restaurants = await _context.Restaurants
            .AsNoTracking()
            .OrderBy(r => r.RestaurantId)
            .ToListAsync();
        return restaurants.Select(Map).ToList();
    }

    public async Task<RestaurantResponseDto?> GetByIdAsync(int id)
    {
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RestaurantId == id);
        return restaurant is null ? null : Map(restaurant);
    }

    public async Task<RestaurantResponseDto> CreateAsync(RestaurantCreateDto dto)
    {
        ValidateOpeningHours(dto.OpenTime, dto.CloseTime);

        var restaurant = new Restaurant
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            Phone = dto.Phone.Trim(),
            OpenTime = dto.OpenTime,
            CloseTime = dto.CloseTime
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();
        return Map(restaurant);
    }

    public async Task<bool> UpdateAsync(int id, RestaurantUpdateDto dto)
    {
        ValidateOpeningHours(dto.OpenTime, dto.CloseTime);

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant is null)
            return false;

        restaurant.Name = dto.Name.Trim();
        restaurant.Address = dto.Address.Trim();
        restaurant.Phone = dto.Phone.Trim();
        restaurant.OpenTime = dto.OpenTime;
        restaurant.CloseTime = dto.CloseTime;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant is null)
            return false;

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();
        return true;
    }

    private static void ValidateOpeningHours(TimeOnly openTime, TimeOnly closeTime)
    {
        if (closeTime <= openTime)
            throw new InvalidOperationException("Closing time must be later than opening time.");
    }

    private static RestaurantResponseDto Map(Restaurant restaurant) => new()
    {
        RestaurantId = restaurant.RestaurantId,
        Name = restaurant.Name,
        Address = restaurant.Address ?? string.Empty,
        Phone = restaurant.Phone ?? string.Empty,
        OpenTime = restaurant.OpenTime ?? default,
        CloseTime = restaurant.CloseTime ?? default
    };
}
