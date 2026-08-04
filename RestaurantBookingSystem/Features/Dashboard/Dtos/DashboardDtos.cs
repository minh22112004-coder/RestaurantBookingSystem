using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Features.Dashboard.Dtos
{
    
    public class ReportFilterDto : IValidatableObject
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string GroupBy { get; set; } = "day";

        public int? RestaurantId { get; set; }

        /// Normalizes the date range and defaults to the most recent 30 days.
        public (DateTime From, DateTime To) Normalize()
        {
            var to = (To ?? DateTime.Now.Date).Date.AddDays(1).AddTicks(-1); 
            var from = (From ?? to.AddDays(-30)).Date;
            if (from > to)
                throw new ArgumentException("The start date must be earlier than or equal to the end date.");
            return (from, to);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From.HasValue && To.HasValue && From.Value.Date > To.Value.Date)
                yield return new ValidationResult(
                    "The start date must be earlier than or equal to the end date.",
                    new[] { nameof(From), nameof(To) });
        }
    }

    // Dashboard
    public class DashboardOverviewDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; }
        public double RevenueGrowthPercent { get; set; }

        public int TodayReservations { get; set; }
        public int PendingReservations { get; set; }
        public int ConfirmedReservations { get; set; }
        public int CancelledReservations { get; set; }

        public int TotalTables { get; set; }
        public int OccupiedTablesNow { get; set; }
        public double TableOccupancyPercent { get; set; }

        public int TotalCustomers { get; set; }

        public int NewCustomersThisMonth { get; set; }
    }

    public class RevenueReportItemDto
    {
        public string PeriodLabel { get; set; } = string.Empty; 
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupBy { get; set; } = "day";
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<RevenueReportItemDto> Items { get; set; } = new();
    }

    public class ReservationReportItemDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public int TotalReservations { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
    }

    public class PeakHourDto
    {
        public int Hour { get; set; }
        public int ReservationCount { get; set; }
    }

    public class ReservationReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupBy { get; set; } = "day";
        public int TotalReservations { get; set; }
        public double CancellationRatePercent { get; set; }
        public List<ReservationReportItemDto> Items { get; set; } = new();
        public List<PeakHourDto> PeakHours { get; set; } = new();
    }

    public class TableOccupancyDto
    {
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int TimesReserved { get; set; }
        public double OccupancyRatePercent { get; set; }
    }

    public class TopMenuItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryRevenueDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }


    public class TopCustomerDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int TotalReservations { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class NewCustomersReportItemDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public int NewCustomerCount { get; set; }
    }
}
