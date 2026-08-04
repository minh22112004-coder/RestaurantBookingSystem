using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Dashboard.Dtos;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Dashboard.Services
{
    public class ReportService : IReportService
    {
        private readonly RestaurantReservationDbContext _context;

        private const string PaymentStatusPaid = "Paid";
        private const string ReservationStatusPending = "Pending";
        private const string ReservationStatusConfirmed = "Confirmed";
        private const string ReservationStatusCancelled = "Cancelled";
        private const string ReservationStatusCompleted = "Completed";
        private const string TableStatusOccupied = "Occupied";


        public ReportService(RestaurantReservationDbContext context)
        {
            _context = context;
        }


        public async Task<DashboardOverviewDto> GetOverviewAsync(int? restaurantId = null)
        {
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var todayDateOnly = DateOnly.FromDateTime(today);

            var tablesQuery = _context.DiningTables.AsQueryable();
            if (restaurantId.HasValue)
                tablesQuery = tablesQuery.Where(t => t.RestaurantId == restaurantId.Value);

            var totalTables = await tablesQuery.CountAsync();
            var occupiedNow = await tablesQuery.CountAsync(t => t.Status == TableStatusOccupied);

            var todayOrders = _context.Orders.AsQueryable();
            var yesterdayOrders = _context.Orders.AsQueryable();
            if (restaurantId.HasValue)
            {
                todayOrders = todayOrders.Where(o => o.Reservation != null &&
                    o.Reservation.Table != null && o.Reservation.Table.RestaurantId == restaurantId.Value);
                yesterdayOrders = yesterdayOrders.Where(o => o.Reservation != null &&
                    o.Reservation.Table != null && o.Reservation.Table.RestaurantId == restaurantId.Value);
            }

            var todayRevenue = await todayOrders
                .Where(o => o.PaymentStatus == PaymentStatusPaid
                            && o.CreatedAt >= today && o.CreatedAt < tomorrow)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var yesterdayRevenue = await yesterdayOrders
                .Where(o => o.PaymentStatus == PaymentStatusPaid
                            && o.CreatedAt >= yesterday && o.CreatedAt < today)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            double growth = yesterdayRevenue == 0
                ? (todayRevenue > 0 ? 100 : 0)
                : (double)((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100;

            var todayReservationsQuery = _context.Reservations.Where(r => r.Date == todayDateOnly);
            if (restaurantId.HasValue)
                todayReservationsQuery = todayReservationsQuery.Where(r => r.Table != null &&
                    r.Table.RestaurantId == restaurantId.Value);

            var todayReservations = await todayReservationsQuery.CountAsync();
            var pending = await todayReservationsQuery.CountAsync(r => r.Status == ReservationStatusPending);
            var confirmed = await todayReservationsQuery.CountAsync(r => r.Status == ReservationStatusConfirmed);
            var cancelled = await todayReservationsQuery.CountAsync(r => r.Status == ReservationStatusCancelled);

            var customersQuery = _context.Users
                .Where(u => u.Roles.Any(role => role.RoleName == RoleNames.Customer));
            if (restaurantId.HasValue)
                customersQuery = customersQuery.Where(u => u.Reservations.Any(r => r.Table != null &&
                    r.Table.RestaurantId == restaurantId.Value));
            var totalCustomers = await customersQuery.CountAsync();

            var firstMonth = new DateOnly(today.Year, today.Month, 1);
            var newCustomersThisMonth = await GetNewCustomersCountAsync(firstMonth, todayDateOnly, restaurantId);

            return new DashboardOverviewDto
            {
                TodayRevenue = todayRevenue,
                YesterdayRevenue = yesterdayRevenue,
                RevenueGrowthPercent = Math.Round(growth, 2),
                TodayReservations = todayReservations,
                PendingReservations = pending,
                ConfirmedReservations = confirmed,
                CancelledReservations = cancelled,
                TotalTables = totalTables,
                OccupiedTablesNow = occupiedNow,
                TableOccupancyPercent = totalTables == 0 ? 0 : Math.Round((double)occupiedNow / totalTables * 100, 2),
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth
            };
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(ReportFilterDto filter)
        {
            var (from, to) = filter.Normalize();

            var query = _context.Orders
                .Include(o => o.Reservation)
                    .ThenInclude(r => r!.Table)
                .Where(o => o.PaymentStatus == PaymentStatusPaid
                            && o.CreatedAt >= from && o.CreatedAt <= to);

            if (filter.RestaurantId.HasValue)
            {
                query = query.Where(o => o.Reservation != null
                                          && o.Reservation.Table != null
                                          && o.Reservation.Table.RestaurantId == filter.RestaurantId.Value);
            }

            var orders = await query
                .Select(o => new { o.TotalAmount, o.CreatedAt })
                .ToListAsync();

            var grouped = orders
                .GroupBy(o => GetPeriodLabel(o.CreatedAt!.Value, filter.GroupBy))
                .OrderBy(g => g.Key)
                .Select(g => new RevenueReportItemDto
                {
                    PeriodLabel = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount ?? 0),
                    OrderCount = g.Count()
                })
                .ToList();

            return new RevenueReportDto
            {
                FromDate = from,
                ToDate = to,
                GroupBy = filter.GroupBy,
                TotalRevenue = orders.Sum(o => o.TotalAmount ?? 0),
                TotalOrders = orders.Count,
                Items = grouped
            };
        }


        public async Task<ReservationReportDto> GetReservationReportAsync(ReportFilterDto filter)
        {
            var (from, to) = filter.Normalize();
            var fromDateOnly = DateOnly.FromDateTime(from);
            var toDateOnly = DateOnly.FromDateTime(to);

            var query = _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.Date >= fromDateOnly && r.Date <= toDateOnly);

            if (filter.RestaurantId.HasValue)
                query = query.Where(r => r.Table != null && r.Table.RestaurantId == filter.RestaurantId.Value);

            var reservations = await query
                .Select(r => new { r.Date, r.StartTime, r.Status })
                .ToListAsync();

            var grouped = reservations
                .GroupBy(r => GetPeriodLabel(r.Date.ToDateTime(TimeOnly.MinValue), filter.GroupBy))
                .OrderBy(g => g.Key)
                .Select(g => new ReservationReportItemDto
                {
                    PeriodLabel = g.Key,
                    TotalReservations = g.Count(),
                    Confirmed = g.Count(x => x.Status == ReservationStatusConfirmed),
                    Cancelled = g.Count(x => x.Status == ReservationStatusCancelled),
                    Completed = g.Count(x => x.Status == ReservationStatusCompleted),
                    Pending = g.Count(x => x.Status == ReservationStatusPending)
                })
                .ToList();

            var peakHours = reservations
                .GroupBy(r => r.StartTime.Hour)
                .OrderBy(g => g.Key)
                .Select(g => new PeakHourDto { Hour = g.Key, ReservationCount = g.Count() })
                .ToList();

            int total = reservations.Count;
            int cancelledCount = reservations.Count(r => r.Status == ReservationStatusCancelled);

            return new ReservationReportDto
            {
                FromDate = from,
                ToDate = to,
                GroupBy = filter.GroupBy,
                TotalReservations = total,
                CancellationRatePercent = total == 0 ? 0 : Math.Round((double)cancelledCount / total * 100, 2),
                Items = grouped,
                PeakHours = peakHours
            };
        }

        public async Task<List<TableOccupancyDto>> GetTableOccupancyReportAsync(ReportFilterDto filter)
        {
            var (from, to) = filter.Normalize();
            var fromDateOnly = DateOnly.FromDateTime(from);
            var toDateOnly = DateOnly.FromDateTime(to);
            int totalDays = Math.Max(1, toDateOnly.DayNumber - fromDateOnly.DayNumber + 1);

            var tablesQuery = _context.DiningTables
                .Include(t => t.Reservations)
                .Include(t => t.Restaurant)
                .AsQueryable();

            if (filter.RestaurantId.HasValue)
                tablesQuery = tablesQuery.Where(t => t.RestaurantId == filter.RestaurantId.Value);

            var tables = await tablesQuery.ToListAsync();

            var result = tables.Select(t =>
            {
                var validReservations = t.Reservations.Where(r =>
                    r.Date >= fromDateOnly && r.Date <= toDateOnly &&
                    r.Status != ReservationStatusCancelled && r.EndTime > r.StartTime).ToList();
                var count = validReservations.Count;
                var reservedMinutes = validReservations.Sum(r =>
                    (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalMinutes);
                var openMinutesPerDay = t.Restaurant?.OpenTime is TimeOnly open &&
                                        t.Restaurant.CloseTime is TimeOnly close && close > open
                    ? (close.ToTimeSpan() - open.ToTimeSpan()).TotalMinutes
                    : TimeSpan.FromDays(1).TotalMinutes;
                var availableMinutes = totalDays * openMinutesPerDay;

                return new TableOccupancyDto
                {
                    TableId = t.TableId,
                    TableNumber = t.TableNumber,
                    TimesReserved = count,
                    OccupancyRatePercent = availableMinutes <= 0
                        ? 0
                        : Math.Round(Math.Min(100, reservedMinutes / availableMinutes * 100), 2)
                };
            })
            .OrderByDescending(x => x.TimesReserved)
            .ToList();

            return result;
        }

        public async Task<List<TopMenuItemDto>> GetTopSellingMenuItemsAsync(ReportFilterDto filter, int top = 10)
        {
            var (from, to) = filter.Normalize();

            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                    .ThenInclude(mi => mi!.Category)
                .Where(oi => oi.Order != null
                             && oi.Order.PaymentStatus == PaymentStatusPaid
                             && oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to
                             && oi.MenuItem != null);

            if (filter.RestaurantId.HasValue)
                query = query.Where(oi => oi.MenuItem!.RestaurantId == filter.RestaurantId.Value);

            var orderItems = await query.ToListAsync();

            var result = orderItems
                .GroupBy(oi => new
                {
                    oi.MenuItem!.MenuItemId,
                    oi.MenuItem.Name,
                    CategoryName = oi.MenuItem.Category?.Name ?? "Uncategorized"
                })
                .Select(g => new TopMenuItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    Name = g.Key.Name,
                    CategoryName = g.Key.CategoryName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToList();

            return result;
        }

        public async Task<List<CategoryRevenueDto>> GetRevenueByCategoryAsync(ReportFilterDto filter)
        {
            var (from, to) = filter.Normalize();

            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                    .ThenInclude(mi => mi!.Category)
                .Where(oi => oi.Order != null
                             && oi.Order.PaymentStatus == PaymentStatusPaid
                             && oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to
                             && oi.MenuItem != null && oi.MenuItem.Category != null);

            if (filter.RestaurantId.HasValue)
                query = query.Where(oi => oi.MenuItem!.RestaurantId == filter.RestaurantId.Value);

            var orderItems = await query.ToListAsync();

            return orderItems
                .GroupBy(oi => new { oi.MenuItem!.Category!.CategoryId, oi.MenuItem.Category.Name })
                .Select(g => new CategoryRevenueDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();
        }

        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(ReportFilterDto filter, int top = 10)
        {
            var (from, to) = filter.Normalize();
            var fromDateOnly = DateOnly.FromDateTime(from);
            var toDateOnly = DateOnly.FromDateTime(to);

            var reservationsQuery = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Order)
                .Where(r => r.Date >= fromDateOnly && r.Date <= toDateOnly && r.UserId != null && r.User != null);
            if (filter.RestaurantId.HasValue)
                reservationsQuery = reservationsQuery.Where(r => r.Table != null &&
                    r.Table.RestaurantId == filter.RestaurantId.Value);
            var reservations = await reservationsQuery.ToListAsync();

            var result = reservations
                .GroupBy(r => new { UserId = r.UserId!.Value, Username = r.User!.Username, r.User.Email })
                .Select(g => new TopCustomerDto
                {
                    UserId = g.Key.UserId,
                    Username = g.Key.Username,
                    Email = g.Key.Email,
                    TotalReservations = g.Count(),
                    TotalSpent = g.Sum(r => (r.Order != null && r.Order.PaymentStatus == PaymentStatusPaid)
                                              ? (r.Order.TotalAmount ?? 0)
                                              : 0)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(top)
                .ToList();

            return result;
        }

        public async Task<List<NewCustomersReportItemDto>> GetNewCustomersReportAsync(ReportFilterDto filter)
        {
            var (from, to) = filter.Normalize();
            var fromDateOnly = DateOnly.FromDateTime(from);
            var toDateOnly = DateOnly.FromDateTime(to);

            var firstBookingDates = await GetFirstBookingDatesAsync(filter.RestaurantId);

            var inRange = firstBookingDates
                .Where(d => d >= fromDateOnly && d <= toDateOnly)
                .ToList();

            return inRange
                .GroupBy(d => GetPeriodLabel(d.ToDateTime(TimeOnly.MinValue), filter.GroupBy))
                .OrderBy(g => g.Key)
                .Select(g => new NewCustomersReportItemDto
                {
                    PeriodLabel = g.Key,
                    NewCustomerCount = g.Count()
                })
                .ToList();
        }

        private async Task<int> GetNewCustomersCountAsync(DateOnly from, DateOnly to, int? restaurantId)
        {
            var firstBookingDates = await GetFirstBookingDatesAsync(restaurantId);
            return firstBookingDates.Count(d => d >= from && d <= to);
        }

        private async Task<List<DateOnly>> GetFirstBookingDatesAsync(int? restaurantId)
        {
            var query = _context.Reservations.Where(r => r.UserId != null);
            if (restaurantId.HasValue)
                query = query.Where(r => r.Table != null && r.Table.RestaurantId == restaurantId.Value);
            var userBookingDates = await query
                .Select(r => new { r.UserId, r.Date })
                .ToListAsync();

            return userBookingDates
                .GroupBy(r => r.UserId)
                .Select(g => g.Min(x => x.Date))
                .ToList();
        }

        private static string GetPeriodLabel(DateTime date, string groupBy)
        {
            return groupBy?.ToLower() switch
            {
                "month" => date.ToString("yyyy-MM"),
                "year" => date.ToString("yyyy"),
                "week" => $"{ISOWeek.GetYear(date)}-W{ISOWeek.GetWeekOfYear(date):D2}",
                _ => date.ToString("yyyy-MM-dd") 
            };
        }
    }
}
