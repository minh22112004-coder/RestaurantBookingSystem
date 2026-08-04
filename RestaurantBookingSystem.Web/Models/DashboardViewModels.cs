using System.Globalization;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Models;

public sealed class DashboardTrendPointViewModel
{
    public DateOnly Date { get; init; }
    public int TotalReservations { get; init; }
    public int Pending { get; init; }
    public int Confirmed { get; init; }
    public int Cancelled { get; init; }
    public double BarPercent { get; init; }
    public string BarPercentCss => BarPercent.ToString("0.##", CultureInfo.InvariantCulture);
}

public sealed class AdminDashboardViewModel
{
    public DashboardOverviewDto Overview { get; init; } = new();
    public IReadOnlyList<RestaurantDto> Restaurants { get; init; } = [];
    public IReadOnlyList<DashboardTrendPointViewModel> Trend { get; init; } = [];
    public int? RestaurantId { get; init; }
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasTrendData => Trend.Any(item => item.TotalReservations > 0);
    public string SelectedRestaurantName => RestaurantId.HasValue
        ? Restaurants.FirstOrDefault(item => item.RestaurantId == RestaurantId)?.Name ?? "Selected restaurant"
        : "All restaurants";
}
