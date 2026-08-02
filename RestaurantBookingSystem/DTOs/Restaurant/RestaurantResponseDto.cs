namespace RestaurantBookingSystem.DTOs.Restaurant;
public class RestaurantResponseDto
{
    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }
}
