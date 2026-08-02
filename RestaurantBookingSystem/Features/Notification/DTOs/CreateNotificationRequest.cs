using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Features.Notification.DTOs;

public class CreateNotificationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0.")]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
}