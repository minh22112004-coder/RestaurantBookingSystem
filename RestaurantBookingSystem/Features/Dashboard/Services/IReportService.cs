using RestaurantBookingSystem.Features.Dashboard.Dtos;

namespace RestaurantBookingSystem.Features.Dashboard.Services
{
    public interface IReportService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(int? restaurantId = null);

        Task<RevenueReportDto> GetRevenueReportAsync(ReportFilterDto filter);

        Task<ReservationReportDto> GetReservationReportAsync(ReportFilterDto filter);

        Task<List<TableOccupancyDto>> GetTableOccupancyReportAsync(ReportFilterDto filter);

        Task<List<TopMenuItemDto>> GetTopSellingMenuItemsAsync(ReportFilterDto filter, int top = 10);

        Task<List<CategoryRevenueDto>> GetRevenueByCategoryAsync(ReportFilterDto filter);

        Task<List<TopCustomerDto>> GetTopCustomersAsync(ReportFilterDto filter, int top = 10);

        Task<List<NewCustomersReportItemDto>> GetNewCustomersReportAsync(ReportFilterDto filter);
    }
}
