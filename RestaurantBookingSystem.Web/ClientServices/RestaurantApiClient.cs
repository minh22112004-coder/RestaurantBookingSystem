using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IRestaurantApiClient
{
    Task<List<RestaurantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RestaurantDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RestaurantDto> CreateAsync(RestaurantWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, RestaurantWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class RestaurantApiClient : ApiClientBase, IRestaurantApiClient
{
    public RestaurantApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<List<RestaurantDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<RestaurantDto>>("api/Restaurant", cancellationToken);

    public Task<RestaurantDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<RestaurantDto>($"api/Restaurant/{id}", cancellationToken);

    public Task<RestaurantDto> CreateAsync(RestaurantWriteRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<RestaurantWriteRequest, RestaurantDto>("api/Restaurant", request, cancellationToken);

    public Task UpdateAsync(int id, RestaurantWriteRequest request, CancellationToken cancellationToken = default) =>
        PutAsync($"api/Restaurant/{id}", request, cancellationToken);

    Task IRestaurantApiClient.DeleteAsync(int id, CancellationToken cancellationToken) =>
        DeleteAsync($"api/Restaurant/{id}", cancellationToken);
}
