namespace RestaurantBookingSystem.Models;
public class DiningTablefc2
{
    public int TableId { get; set; }

    public int RestaurantId { get; set; }

    public string TableNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = "Available";
}
