using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Web.Contracts;

public sealed class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthenticatedUser User { get; set; } = new();
}

public sealed class AuthenticatedUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class CurrentUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public sealed class RestaurantDto
{
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}

public sealed class RestaurantWriteRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
}

public sealed class DiningTableDto
{
    public int TableId { get; set; }
    public int RestaurantId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class DiningTableWriteRequest
{
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    [Required, StringLength(20)]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Capacity { get; set; }

    [Required]
    public string Status { get; set; } = "Available";
}

public sealed class ReservationDto
{
    public int ReservationId { get; set; }
    public int UserId { get; set; }
    public int TableId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GuestCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class ReservationWriteRequest
{
    [Range(1, int.MaxValue)]
    public int TableId { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [Range(1, 100)]
    public int GuestCount { get; set; }
}

public sealed class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CategoryWriteRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public sealed class MenuItemDto
{
    public int MenuItemId { get; set; }
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Available { get; set; }
}

public sealed class MenuItemWriteRequest
{
    [Range(1, int.MaxValue)]
    public int RestaurantId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }

    public bool Available { get; set; } = true;
}

public sealed class NotificationDto
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateNotificationRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;
}

public sealed class DashboardOverviewDto
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

public sealed class ReservationReportItemDto
{
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalReservations { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
}

public sealed class ReservationReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string GroupBy { get; set; } = "day";
    public int TotalReservations { get; set; }
    public double CancellationRatePercent { get; set; }
    public List<ReservationReportItemDto> Items { get; set; } = [];
}

public sealed class ReportFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string GroupBy { get; set; } = "day";
    public int? RestaurantId { get; set; }
}
