using System.Text.Json;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class FakeReportApiClient : IReportApiClient
{
    public DashboardOverviewDto Overview { get; set; } = new()
    {
        TodayRevenue = 1250000,
        YesterdayRevenue = 1000000,
        RevenueGrowthPercent = 25,
        TodayReservations = 12,
        PendingReservations = 3,
        ConfirmedReservations = 7,
        CancelledReservations = 2,
        TotalTables = 20,
        OccupiedTablesNow = 8,
        TableOccupancyPercent = 40,
        TotalCustomers = 145,
        NewCustomersThisMonth = 9
    };

    public ReservationReportDto ReservationReport { get; set; } = CreateReservationReport();
    public ApiClientException? OverviewException { get; set; }
    public ApiClientException? ReservationException { get; set; }
    public int? LastOverviewRestaurantId { get; private set; }
    public ReportFilter? LastReservationFilter { get; private set; }

    public Task<DashboardOverviewDto> GetOverviewAsync(int? restaurantId = null, CancellationToken cancellationToken = default)
    {
        LastOverviewRestaurantId = restaurantId;
        return OverviewException is null
            ? Task.FromResult(Overview)
            : Task.FromException<DashboardOverviewDto>(OverviewException);
    }

    public Task<ReservationReportDto> GetReservationsAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        LastReservationFilter = filter;
        return ReservationException is null
            ? Task.FromResult(ReservationReport)
            : Task.FromException<ReservationReportDto>(ReservationException);
    }

    public Task<JsonElement> GetRevenueAsync(ReportFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));
    public Task<JsonElement> GetTableOccupancyAsync(ReportFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));
    public Task<JsonElement> GetTopMenuItemsAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));
    public Task<JsonElement> GetRevenueByCategoryAsync(ReportFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));
    public Task<JsonElement> GetTopCustomersAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));
    public Task<JsonElement> GetNewCustomersAsync(ReportFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(default(JsonElement));

    private static ReservationReportDto CreateReservationReport()
    {
        var today = DateTime.Today;
        return new ReservationReportDto
        {
            FromDate = today.AddDays(-6),
            ToDate = today,
            GroupBy = "day",
            TotalReservations = 9,
            Items =
            [
                new() { PeriodLabel = today.AddDays(-2).ToString("yyyy-MM-dd"), TotalReservations = 4, Pending = 1, Confirmed = 2, Cancelled = 1 },
                new() { PeriodLabel = today.AddDays(-1).ToString("yyyy-MM-dd"), TotalReservations = 5, Pending = 2, Confirmed = 3 }
            ]
        };
    }
}
