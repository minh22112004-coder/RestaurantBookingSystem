using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.DiningTable;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Services;

public class DiningTableService : IDiningTableService
{
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Available", "Reserved", "Occupied", "Maintenance" };

    private readonly RestaurantReservationDbContext _context;

    public DiningTableService(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DiningTableResponseDto>> GetAllAsync()
    {
        var tables = await _context.DiningTables.AsNoTracking()
            .OrderBy(t => t.TableId)
            .ToListAsync();
        return tables.Select(Map).ToList();
    }

    public async Task<DiningTableResponseDto?> GetByIdAsync(int id)
    {
        var table = await _context.DiningTables.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TableId == id);
        return table is null ? null : Map(table);
    }

    public async Task<IReadOnlyList<DiningTableResponseDto>> GetByRestaurantAsync(int restaurantId)
    {
        var tables = await _context.DiningTables.AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId)
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
        return tables.Select(Map).ToList();
    }

    public async Task<DiningTableResponseDto> CreateAsync(DiningTableCreateDto dto)
    {
        await ValidateAsync(dto.RestaurantId, dto.TableNumber, dto.Status);

        var table = new DiningTable
        {
            RestaurantId = dto.RestaurantId,
            TableNumber = dto.TableNumber.Trim(),
            Capacity = dto.Capacity,
            Status = NormalizeStatus(dto.Status)
        };

        _context.DiningTables.Add(table);
        await _context.SaveChangesAsync();
        return Map(table);
    }

    public async Task<bool> UpdateAsync(int id, DiningTableUpdateDto dto)
    {
        var table = await _context.DiningTables.FindAsync(id);
        if (table is null)
            return false;

        await ValidateAsync(dto.RestaurantId, dto.TableNumber, dto.Status, id);
        table.RestaurantId = dto.RestaurantId;
        table.TableNumber = dto.TableNumber.Trim();
        table.Capacity = dto.Capacity;
        table.Status = NormalizeStatus(dto.Status);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var table = await _context.DiningTables.FindAsync(id);
        if (table is null)
            return false;

        _context.DiningTables.Remove(table);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task ValidateAsync(int restaurantId, string tableNumber, string status, int? excludedId = null)
    {
        if (!await _context.Restaurants.AnyAsync(r => r.RestaurantId == restaurantId))
            throw new KeyNotFoundException("Restaurant not found.");
        if (!AllowedStatuses.Contains(status))
            throw new InvalidOperationException("The table status is invalid.");

        var normalizedNumber = tableNumber.Trim();
        var duplicate = await _context.DiningTables.AnyAsync(t =>
            t.RestaurantId == restaurantId && t.TableNumber == normalizedNumber && t.TableId != excludedId);
        if (duplicate)
            throw new InvalidOperationException("The table number already exists in this restaurant.");
    }

    private static string NormalizeStatus(string status) =>
        AllowedStatuses.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

    private static DiningTableResponseDto Map(DiningTable table) => new()
    {
        TableId = table.TableId,
        RestaurantId = table.RestaurantId ?? 0,
        TableNumber = table.TableNumber,
        Capacity = table.Capacity,
        Status = table.Status ?? "Available"
    };
}
