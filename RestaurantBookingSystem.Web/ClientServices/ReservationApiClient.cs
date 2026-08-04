using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IReservationApiClient
{
    Task<List<ReservationDto>> GetMineAsync(CancellationToken cancellationToken = default);
    Task<List<ReservationDto>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<ReservationDto> CreateAsync(ReservationWriteRequest request, CancellationToken cancellationToken = default);
    Task<ReservationDto> UpdateAsync(int id, ReservationWriteRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class ReservationApiClient : ApiClientBase, IReservationApiClient
{
    public ReservationApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<List<ReservationDto>> GetMineAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<ReservationDto>>("api/Reservation/mine", cancellationToken);

    public Task<List<ReservationDto>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        GetAsync<List<ReservationDto>>($"api/Reservation/date/{date:yyyy-MM-dd}", cancellationToken);

    public Task<ReservationDto> CreateAsync(ReservationWriteRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<ReservationWriteRequest, ReservationDto>("api/Reservation", request, cancellationToken);

    public Task<ReservationDto> UpdateAsync(int id, ReservationWriteRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<ReservationWriteRequest, ReservationDto>($"api/Reservation/{id}", request, cancellationToken);

    public Task CancelAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync($"api/Reservation/{id}/cancel", cancellationToken);
}
