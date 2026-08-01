using System;
using System.Collections.Generic;

namespace RestaurantBookingSystem.Models;

public partial class DiningTable
{
    public int TableId { get; set; }

    public int? RestaurantId { get; set; }

    public string TableNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual Restaurant? Restaurant { get; set; }
}
