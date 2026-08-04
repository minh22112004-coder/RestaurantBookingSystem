using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Models;

public sealed class RestaurantListViewModel
{
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public string SearchTerm { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}

public sealed class RestaurantDetailsViewModel
{
    public RestaurantDto? Restaurant { get; init; }
    public IReadOnlyList<MenuItemDto> MenuItems { get; init; } = [];
    public IReadOnlyList<DiningTableDto> Tables { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public ReservationFormViewModel BookingForm { get; init; } = new();
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public IEnumerable<IGrouping<string, MenuItemDto>> MenuGroups =>
        MenuItems
            .OrderBy(item => item.CategoryName)
            .ThenBy(item => item.Name)
            .GroupBy(item => string.IsNullOrWhiteSpace(item.CategoryName) ? "Other" : item.CategoryName);
}
