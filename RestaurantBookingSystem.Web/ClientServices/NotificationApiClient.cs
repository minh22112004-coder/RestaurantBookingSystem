using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface INotificationApiClient
{
    Task<List<NotificationDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class NotificationApiClient : ApiClientBase, INotificationApiClient
{
    public NotificationApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<List<NotificationDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        GetAsync<List<NotificationDto>>($"api/notifications/user/{userId}", cancellationToken);

    public Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<CreateNotificationRequest, NotificationDto>("api/notifications", request, cancellationToken);

    public Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync($"api/notifications/{id}/read", cancellationToken);
}
