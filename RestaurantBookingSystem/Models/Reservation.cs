using System;
using System.Collections.Generic;

namespace RestaurantBookingSystem.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int? UserId { get; set; }

    public int? TableId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int GuestCount { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }

    public virtual DiningTable? Table { get; set; }

    public virtual User? User { get; set; }
}
