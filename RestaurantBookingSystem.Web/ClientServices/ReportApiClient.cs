using System.Globalization;
using System.Text.Json;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IReportApiClient
{
    Task<DashboardOverviewDto> GetOverviewAsync(int? restaurantId = null, CancellationToken cancellationToken = default);
    Task<JsonElement> GetRevenueAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<ReservationReportDto> GetReservationsAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<JsonElement> GetTableOccupancyAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<JsonElement> GetTopMenuItemsAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default);
    Task<JsonElement> GetRevenueByCategoryAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    Task<JsonElement> GetTopCustomersAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default);
    Task<JsonElement> GetNewCustomersAsync(ReportFilter filter, CancellationToken cancellationToken = default);
}

public sealed class ReportApiClient : ApiClientBase, IReportApiClient
{
    public ReportApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<DashboardOverviewDto> GetOverviewAsync(int? restaurantId = null, CancellationToken cancellationToken = default) =>
        GetAsync<DashboardOverviewDto>(
            restaurantId.HasValue ? $"api/dashboard/overview?restaurantId={restaurantId.Value}" : "api/dashboard/overview",
            cancellationToken);

    public Task<JsonElement> GetRevenueAsync(ReportFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/revenue{BuildQuery(filter)}", cancellationToken);

    public Task<ReservationReportDto> GetReservationsAsync(ReportFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<ReservationReportDto>($"api/reports/reservations{BuildQuery(filter)}", cancellationToken);

    public Task<JsonElement> GetTableOccupancyAsync(ReportFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/tables/occupancy{BuildQuery(filter)}", cancellationToken);

    public Task<JsonElement> GetTopMenuItemsAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/menu-items/top-selling{BuildQuery(filter, top)}", cancellationToken);

    public Task<JsonElement> GetRevenueByCategoryAsync(ReportFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/menu-items/by-category{BuildQuery(filter)}", cancellationToken);

    public Task<JsonElement> GetTopCustomersAsync(ReportFilter filter, int top = 10, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/customers/top{BuildQuery(filter, top)}", cancellationToken);

    public Task<JsonElement> GetNewCustomersAsync(ReportFilter filter, CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>($"api/reports/customers/new{BuildQuery(filter)}", cancellationToken);

    private static string BuildQuery(ReportFilter filter, int? top = null)
    {
        var values = new List<string>();
        if (filter.From.HasValue)
            values.Add($"from={Uri.EscapeDataString(filter.From.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (filter.To.HasValue)
            values.Add($"to={Uri.EscapeDataString(filter.To.Value.ToString("O", CultureInfo.InvariantCulture))}");
        if (!string.IsNullOrWhiteSpace(filter.GroupBy))
            values.Add($"groupBy={Uri.EscapeDataString(filter.GroupBy)}");
        if (filter.RestaurantId.HasValue)
            values.Add($"restaurantId={filter.RestaurantId.Value}");
        if (top.HasValue)
            values.Add($"top={top.Value}");
        return values.Count == 0 ? string.Empty : $"?{string.Join('&', values)}";
    }
}
