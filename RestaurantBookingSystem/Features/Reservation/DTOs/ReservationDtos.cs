using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Features.Reservation.DTOs;

public class CreateReservationDto
{
    [Range(1, int.MaxValue)]
    public int TableId { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [Range(1, 100)]
    public int GuestCount { get; set; }
}

public class UpdateReservationDto
{
    [Range(1, int.MaxValue)]
    public int TableId { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [Range(1, 100)]
    public int GuestCount { get; set; }
}

public class ReservationResponseDto
{
    public int ReservationId { get; set; }
    public int UserId { get; set; }
    public int TableId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GuestCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
