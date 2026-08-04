using System.ComponentModel.DataAnnotations;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Models;

public sealed class ReservationFormViewModel : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a table.")]
    [Display(Name = "Table")]
    public int TableId { get; set; }

    [Required(ErrorMessage = "Please choose a reservation date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "Please choose a start time.")]
    [DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "Please choose an end time.")]
    [DataType(DataType.Time)]
    [Display(Name = "End time")]
    public TimeOnly EndTime { get; set; }

    [Range(1, 100, ErrorMessage = "The guest count must be between 1 and 100.")]
    [Display(Name = "Guests")]
    public int GuestCount { get; set; } = 2;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Date < DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("Reservations cannot be made in the past.", [nameof(Date)]);

        if (EndTime <= StartTime)
            yield return new ValidationResult("The end time must be later than the start time.", [nameof(EndTime)]);
    }
}

public sealed class ReservationListItemViewModel
{
    public required ReservationDto Reservation { get; init; }
    public string RestaurantName { get; init; } = "Restaurant unavailable";
    public string TableNumber { get; init; } = string.Empty;
    public int TableCapacity { get; init; }
    public bool IsCancelled => string.Equals(Reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase);
}

public sealed class ReservationListViewModel
{
    public IReadOnlyList<ReservationListItemViewModel> Items { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}

public sealed class ReservationEditViewModel
{
    public int ReservationId { get; init; }
    public string RestaurantName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public TimeOnly OpenTime { get; init; }
    public TimeOnly CloseTime { get; init; }
    public ReservationFormViewModel Form { get; init; } = new();
    public IReadOnlyList<DiningTableDto> Tables { get; init; } = [];
}

public sealed class NotificationListViewModel
{
    public IReadOnlyList<NotificationDto> Notifications { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public int UnreadCount => Notifications.Count(notification => !notification.IsRead);
}
