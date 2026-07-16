using System;
using System.Collections.Generic;

namespace RestaurantBookingSystem.Models;

public partial class Restaurant
{
    public int RestaurantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public TimeOnly? OpenTime { get; set; }

    public TimeOnly? CloseTime { get; set; }

    public virtual ICollection<DiningTable> DiningTables { get; set; } = new List<DiningTable>();

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
