using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantBookingSystem.Features.Notification.DTOs;
using RestaurantBookingSystem.Features.Notification.Services;
using RestaurantBookingSystem.Features.Reservation.DTOs;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Reservation.Services;

public class ReservationService
{
    private readonly RestaurantReservationDbContext _context;
    private readonly INotificationService _notificationService;

    public ReservationService(
        RestaurantReservationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> IsTableAvailableAsync(
        int tableId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludedReservationId = null)
    {
        return !await _context.Reservations.AnyAsync(r =>
            r.TableId == tableId &&
            r.Date == date &&
            r.ReservationId != excludedReservationId &&
            r.Status != "Cancelled" &&
            r.StartTime < endTime &&
            r.EndTime > startTime);
    }

    public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto, int userId)
    {
        ValidateTime(dto.Date, dto.StartTime, dto.EndTime);
        if (!await _context.Users.AnyAsync(u => u.UserId == userId))
            throw new KeyNotFoundException("User not found.");

        IDbContextTransaction? transaction = null;
        if (_context.Database.IsRelational())
            transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var table = await GetAndValidateTableAsync(dto.TableId, dto.GuestCount, dto.StartTime, dto.EndTime);
            if (!await IsTableAvailableAsync(dto.TableId, dto.Date, dto.StartTime, dto.EndTime))
                throw new InvalidOperationException("The table is already reserved during this time slot.");

            var reservation = new Models.Reservation
            {
                UserId = userId,
                TableId = dto.TableId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                GuestCount = dto.GuestCount,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            await CreateNotificationAsync(
                userId,
                "Reservation request received",
                $"Your request for table {table.TableNumber} on {dto.Date:dd/MM/yyyy} from {dto.StartTime:HH:mm} to {dto.EndTime:HH:mm} is awaiting confirmation.");

            if (transaction is not null)
                await transaction.CommitAsync();
            return Map(reservation);
        }
        catch
        {
            if (transaction is not null)
                await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetReservationsByCustomerAsync(int userId)
    {
        var reservations = await _context.Reservations.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToListAsync();
        return reservations.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetReservationsByDateAsync(DateOnly date)
    {
        var reservations = await _context.Reservations.AsNoTracking()
            .Where(r => r.Date == date)
            .OrderBy(r => r.StartTime)
            .ToListAsync();
        return reservations.Select(Map).ToList();
    }

    public async Task CancelReservationAsync(int reservationId, int userId, bool canManageAll)
    {
        var reservation = await GetOwnedReservationAsync(reservationId, userId, canManageAll);
        if (reservation.Status == "Cancelled")
            return;

        reservation.Status = "Cancelled";
        await _context.SaveChangesAsync();
        await CreateNotificationAsync(
            reservation.UserId!.Value,
            "Reservation cancelled",
            $"Your reservation on {reservation.Date:dd/MM/yyyy} at {reservation.StartTime:HH:mm} has been cancelled.");
    }

    public async Task<ReservationResponseDto> UpdateReservationAsync(
        int reservationId,
        UpdateReservationDto dto,
        int userId,
        bool canManageAll)
    {
        ValidateTime(dto.Date, dto.StartTime, dto.EndTime);
        var reservation = await GetOwnedReservationAsync(reservationId, userId, canManageAll);
        if (reservation.Status == "Cancelled")
            throw new InvalidOperationException("Cancelled reservations cannot be updated.");

        await GetAndValidateTableAsync(dto.TableId, dto.GuestCount, dto.StartTime, dto.EndTime);
        if (!await IsTableAvailableAsync(dto.TableId, dto.Date, dto.StartTime, dto.EndTime, reservationId))
            throw new InvalidOperationException("The table is already reserved during the new time slot.");

        reservation.TableId = dto.TableId;
        reservation.Date = dto.Date;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.GuestCount = dto.GuestCount;
        await _context.SaveChangesAsync();
        await CreateNotificationAsync(
            reservation.UserId!.Value,
            "Reservation updated",
            $"Your reservation was updated to {dto.Date:dd/MM/yyyy}, from {dto.StartTime:HH:mm} to {dto.EndTime:HH:mm}.");
        return Map(reservation);
    }

    private async Task<DiningTable> GetAndValidateTableAsync(
        int tableId,
        int guestCount,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        var table = await _context.DiningTables
            .Include(t => t.Restaurant)
            .FirstOrDefaultAsync(t => t.TableId == tableId);
        if (table is null)
            throw new KeyNotFoundException("Dining table not found.");
        if (guestCount <= 0 || guestCount > table.Capacity)
            throw new InvalidOperationException($"Guest count must be between 1 and {table.Capacity}.");
        if (string.Equals(table.Status, "Maintenance", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The table is under maintenance and cannot be reserved.");
        if (table.Restaurant?.OpenTime is TimeOnly openTime &&
            table.Restaurant.CloseTime is TimeOnly closeTime &&
            (startTime < openTime || endTime > closeTime))
        {
            throw new InvalidOperationException($"The restaurant is open from {openTime:HH:mm} to {closeTime:HH:mm}.");
        }
        return table;
    }

    private async Task<Models.Reservation> GetOwnedReservationAsync(int reservationId, int userId, bool canManageAll)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation is null)
            throw new KeyNotFoundException("Reservation not found.");
        if (!canManageAll && reservation.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to manage this reservation.");
        return reservation;
    }

    private static void ValidateTime(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidOperationException("Reservations cannot be made in the past.");
        if (endTime <= startTime)
            throw new InvalidOperationException("End time must be later than start time.");
    }

    private async Task CreateNotificationAsync(int userId, string title, string message)
    {
        var result = await _notificationService.CreateAsync(new CreateNotificationRequest
        {
            UserId = userId,
            Title = title,
            Message = message
        });
        if (result is null)
            throw new InvalidOperationException("The notification could not be created for the user.");
    }

    private static ReservationResponseDto Map(Models.Reservation reservation) => new()
    {
        ReservationId = reservation.ReservationId,
        UserId = reservation.UserId ?? 0,
        TableId = reservation.TableId ?? 0,
        Date = reservation.Date,
        StartTime = reservation.StartTime,
        EndTime = reservation.EndTime,
        GuestCount = reservation.GuestCount,
        Status = reservation.Status ?? string.Empty
    };
}
