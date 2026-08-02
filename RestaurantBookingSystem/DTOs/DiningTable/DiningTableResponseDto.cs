namespace RestaurantBookingSystem.DTOs.DiningTable;
public class DiningTableResponseDto
{
    public int TableId { get; set; }

    public int RestaurantId { get; set; }

    public string TableNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = string.Empty;
}
