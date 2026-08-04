using System.ComponentModel.DataAnnotations;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Models;

public sealed class AdminRestaurantFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Restaurant name is required.")]
    [StringLength(100)]
    [Display(Name = "Restaurant name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.Time)]
    [Display(Name = "Opening time")]
    public TimeOnly OpenTime { get; set; } = new(8, 0);

    [DataType(DataType.Time)]
    [Display(Name = "Closing time")]
    public TimeOnly CloseTime { get; set; } = new(22, 0);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CloseTime <= OpenTime)
            yield return new ValidationResult("Closing time must be later than opening time.", [nameof(CloseTime)]);
    }
}

public sealed class AdminRestaurantIndexViewModel
{
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public AdminRestaurantFormViewModel Form { get; init; } = new();
}

public sealed class AdminRestaurantEditViewModel
{
    public int RestaurantId { get; init; }
    public AdminRestaurantFormViewModel Form { get; init; } = new();
}

public sealed class AdminTableFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a restaurant.")]
    [Display(Name = "Restaurant")]
    public int RestaurantId { get; set; }

    [Required(ErrorMessage = "Table number is required.")]
    [StringLength(20)]
    [Display(Name = "Table number")]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20.")]
    public int Capacity { get; set; } = 2;

    [Required]
    public string Status { get; set; } = "Available";
}

public sealed class AdminTableIndexViewModel
{
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<DiningTableDto> Tables { get; init; } = [];
    public int? SelectedRestaurantId { get; init; }
    public AdminTableFormViewModel Form { get; init; } = new();
    public string RestaurantName(int restaurantId) =>
        Restaurants.FirstOrDefault(item => item.RestaurantId == restaurantId)?.Name ?? "Unknown restaurant";
}

public sealed class AdminTableEditViewModel
{
    public int TableId { get; init; }
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public AdminTableFormViewModel Form { get; init; } = new();
}

public sealed class AdminCategoryFormViewModel
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class AdminMenuItemFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a restaurant.")]
    [Display(Name = "Restaurant")]
    public int RestaurantId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Item name is required.")]
    [StringLength(100)]
    [Display(Name = "Item name")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Display(Name = "Available for ordering")]
    public bool Available { get; set; } = true;
}

public sealed class AdminMenuIndexViewModel
{
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<CategoryDto> Categories { get; init; } = [];
    public IReadOnlyList<MenuItemDto> Items { get; init; } = [];
    public int? SelectedRestaurantId { get; init; }
    public AdminCategoryFormViewModel CategoryForm { get; init; } = new();
    public AdminMenuItemFormViewModel ItemForm { get; init; } = new();
}

public sealed class AdminCategoryEditViewModel
{
    public int CategoryId { get; init; }
    public AdminCategoryFormViewModel Form { get; init; } = new();
}

public sealed class AdminMenuItemEditViewModel
{
    public int MenuItemId { get; init; }
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<CategoryDto> Categories { get; init; } = [];
    public AdminMenuItemFormViewModel Form { get; init; } = new();
}

public sealed class AdminReservationRowViewModel
{
    public required ReservationDto Reservation { get; init; }
    public string RestaurantName { get; init; } = "Unknown restaurant";
    public string TableNumber { get; init; } = "Unknown table";
}

public sealed class AdminReservationIndexViewModel
{
    public DateOnly Date { get; init; }
    public int? RestaurantId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<AdminReservationRowViewModel> Reservations { get; init; } = [];
}

public sealed class AdminReservationEditViewModel
{
    public int ReservationId { get; init; }
    public int UserId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<DiningTableDto> Tables { get; init; } = [];
    public ReservationFormViewModel Form { get; init; } = new();
    public string RestaurantName(int restaurantId) =>
        Restaurants.FirstOrDefault(item => item.RestaurantId == restaurantId)?.Name ?? "Unknown restaurant";
}
