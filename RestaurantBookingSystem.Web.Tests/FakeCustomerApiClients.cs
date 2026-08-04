using System.Net;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class FakeReservationApiClient : IReservationApiClient
{
    public List<ReservationDto> Reservations { get; } =
    [
        new() { ReservationId = 10, UserId = 42, TableId = 1, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(2)), StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(20, 0), GuestCount = 2, Status = "Pending" },
        new() { ReservationId = 11, UserId = 42, TableId = 2, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), StartTime = new TimeOnly(19, 0), EndTime = new TimeOnly(21, 0), GuestCount = 4, Status = "Cancelled" }
    ];

    public ReservationWriteRequest? LastCreateRequest { get; private set; }
    public ReservationWriteRequest? LastUpdateRequest { get; private set; }
    public int? LastUpdatedId { get; private set; }
    public int? LastCancelledId { get; private set; }
    public ApiClientException? CreateException { get; set; }
    public ApiClientException? UpdateException { get; set; }

    public Task<List<ReservationDto>> GetMineAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Reservations.ToList());

    public Task<List<ReservationDto>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        Task.FromResult(Reservations.Where(reservation => reservation.Date == date).ToList());

    public Task<ReservationDto> CreateAsync(ReservationWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (CreateException is not null)
            return Task.FromException<ReservationDto>(CreateException);

        LastCreateRequest = request;
        var created = ToDto(Reservations.Max(reservation => reservation.ReservationId) + 1, request);
        Reservations.Add(created);
        return Task.FromResult(created);
    }

    public Task<ReservationDto> UpdateAsync(int id, ReservationWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (UpdateException is not null)
            return Task.FromException<ReservationDto>(UpdateException);

        LastUpdatedId = id;
        LastUpdateRequest = request;
        var existing = Reservations.FirstOrDefault(reservation => reservation.ReservationId == id);
        if (existing is null)
            return Task.FromException<ReservationDto>(new ApiClientException(HttpStatusCode.NotFound, "Not found."));

        existing.TableId = request.TableId;
        existing.Date = request.Date;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.GuestCount = request.GuestCount;
        return Task.FromResult(existing);
    }

    public Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        LastCancelledId = id;
        var existing = Reservations.FirstOrDefault(reservation => reservation.ReservationId == id);
        if (existing is null)
            return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        existing.Status = "Cancelled";
        return Task.CompletedTask;
    }

    private static ReservationDto ToDto(int id, ReservationWriteRequest request) => new()
    {
        ReservationId = id,
        UserId = 42,
        TableId = request.TableId,
        Date = request.Date,
        StartTime = request.StartTime,
        EndTime = request.EndTime,
        GuestCount = request.GuestCount,
        Status = "Pending"
    };
}

public sealed class FakeNotificationApiClient : INotificationApiClient
{
    public List<NotificationDto> Notifications { get; } =
    [
        new() { NotificationId = 1, UserId = 42, Title = "Reservation received", Message = "Your reservation request is waiting for confirmation.", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
        new() { NotificationId = 2, UserId = 42, Title = "Reservation updated", Message = "Your reservation time has been updated.", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
    ];

    public int? LastReadId { get; private set; }

    public Task<List<NotificationDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Notifications.Where(notification => notification.UserId == userId).ToList());

    public Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var notification = Notifications.FirstOrDefault(item => item.NotificationId == id);
        if (notification is null)
            return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        notification.IsRead = true;
        LastReadId = id;
        return Task.CompletedTask;
    }
}
