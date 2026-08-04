using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.DTOs.DiningTable;

public class DiningTableUpdateDto
{
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Capacity { get; set; }

    [Required]
    [RegularExpression("^(Available|Reserved|Occupied|Maintenance)$", ErrorMessage = "The table status is invalid.")]
    public string Status { get; set; } = "Available";
}
