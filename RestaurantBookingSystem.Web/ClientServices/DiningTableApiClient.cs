using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IDiningTableApiClient
{
    Task<List<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<DiningTableDto>> GetByRestaurantAsync(int restaurantId, CancellationToken cancellationToken = default);
    Task<DiningTableDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DiningTableDto> CreateAsync(DiningTableWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, DiningTableWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class DiningTableApiClient : ApiClientBase, IDiningTableApiClient
{
    public DiningTableApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<List<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<DiningTableDto>>("api/DiningTable", cancellationToken);

    public Task<List<DiningTableDto>> GetByRestaurantAsync(int restaurantId, CancellationToken cancellationToken = default) =>
        GetAsync<List<DiningTableDto>>($"api/DiningTable/restaurant/{restaurantId}", cancellationToken);

    public Task<DiningTableDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<DiningTableDto>($"api/DiningTable/{id}", cancellationToken);

    public Task<DiningTableDto> CreateAsync(DiningTableWriteRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<DiningTableWriteRequest, DiningTableDto>("api/DiningTable", request, cancellationToken);

    public Task UpdateAsync(int id, DiningTableWriteRequest request, CancellationToken cancellationToken = default) =>
        PutAsync($"api/DiningTable/{id}", request, cancellationToken);

    Task IDiningTableApiClient.DeleteAsync(int id, CancellationToken cancellationToken) =>
        DeleteAsync($"api/DiningTable/{id}", cancellationToken);
}
