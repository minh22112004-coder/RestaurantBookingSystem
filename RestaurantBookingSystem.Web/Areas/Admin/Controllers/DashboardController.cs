using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequireSessionRole("Admin")]
public sealed class DashboardController : Controller
{
    private readonly IReportApiClient _reports;
    private readonly IRestaurantApiClient _restaurants;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IReportApiClient reports,
        IRestaurantApiClient restaurants,
        ILogger<DashboardController> logger)
    {
        _reports = reports;
        _restaurants = restaurants;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? restaurantId, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var filter = new ReportFilter
        {
            From = today.AddDays(-6),
            To = today,
            GroupBy = "day",
            RestaurantId = restaurantId
        };

        try
        {
            var overviewTask = _reports.GetOverviewAsync(restaurantId, cancellationToken);
            var trendTask = _reports.GetReservationsAsync(filter, cancellationToken);
            var restaurantsTask = _restaurants.GetAllAsync(cancellationToken);
            await Task.WhenAll(overviewTask, trendTask, restaurantsTask);

            return View(new AdminDashboardViewModel
            {
                Overview = await overviewTask,
                Restaurants = await restaurantsTask,
                RestaurantId = restaurantId,
                Trend = BuildTrend(await trendTask, DateOnly.FromDateTime(today.AddDays(-6)), DateOnly.FromDateTime(today))
            });
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load the Admin dashboard.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(new AdminDashboardViewModel
            {
                RestaurantId = restaurantId,
                ErrorMessage = "Dashboard data is temporarily unavailable. Please try again shortly."
            });
        }
    }

    private static IReadOnlyList<DashboardTrendPointViewModel> BuildTrend(
        ReservationReportDto report,
        DateOnly from,
        DateOnly to)
    {
        var items = report.Items.ToDictionary(item => item.PeriodLabel, StringComparer.Ordinal);
        var values = new List<(DateOnly Date, ReservationReportItemDto Item)>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            items.TryGetValue(date.ToString("yyyy-MM-dd"), out var item);
            values.Add((date, item ?? new ReservationReportItemDto { PeriodLabel = date.ToString("yyyy-MM-dd") }));
        }

        var maximum = Math.Max(1, values.Max(value => value.Item.TotalReservations));
        return values.Select(value => new DashboardTrendPointViewModel
        {
            Date = value.Date,
            TotalReservations = value.Item.TotalReservations,
            Pending = value.Item.Pending,
            Confirmed = value.Item.Confirmed,
            Cancelled = value.Item.Cancelled,
            BarPercent = Math.Round((double)value.Item.TotalReservations / maximum * 100, 2)
        }).ToList();
    }
}
