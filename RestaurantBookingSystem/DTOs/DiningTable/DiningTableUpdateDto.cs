using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.DTOs.DiningTable;

public class DiningTableUpdateDto
{
    [Required]
    public int RestaurantId { get; set; }

    [Required]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Capacity { get; set; }

    [Required]
    public string Status { get; set; } = "Available";
}