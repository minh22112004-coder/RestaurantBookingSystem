using System;
using System.Collections.Generic;

namespace RestaurantBookingSystem.Models;

public partial class MenuItem
{
    public int MenuItemId { get; set; }

    public int? RestaurantId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? Available { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Restaurant? Restaurant { get; set; }
}
