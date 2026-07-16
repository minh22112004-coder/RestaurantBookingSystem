using System;
using System.Collections.Generic;

namespace RestaurantBookingSystem.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? ReservationId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Reservation? Reservation { get; set; }
}
