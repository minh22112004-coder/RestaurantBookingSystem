using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IMenuApiClient
{
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CategoryWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(int id, CategoryWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MenuItemDto>> GetItemsAsync(int? restaurantId = null, CancellationToken cancellationToken = default);
    Task<MenuItemDto> CreateItemAsync(MenuItemWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateItemAsync(int id, MenuItemWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class MenuApiClient : ApiClientBase, IMenuApiClient
{
    public MenuApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<CategoryDto>>("api/Category", cancellationToken);

    public Task<CategoryDto> CreateCategoryAsync(CategoryWriteRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<CategoryWriteRequest, CategoryDto>("api/Category", request, cancellationToken);

    public Task UpdateCategoryAsync(int id, CategoryWriteRequest request, CancellationToken cancellationToken = default) =>
        PutAsync($"api/Category/{id}", request, cancellationToken);

    public Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/Category/{id}", cancellationToken);

    public Task<List<MenuItemDto>> GetItemsAsync(int? restaurantId = null, CancellationToken cancellationToken = default) =>
        GetAsync<List<MenuItemDto>>(
            restaurantId.HasValue ? $"api/MenuItem?restaurantId={restaurantId.Value}" : "api/MenuItem",
            cancellationToken);

    public Task<MenuItemDto> CreateItemAsync(MenuItemWriteRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<MenuItemWriteRequest, MenuItemDto>("api/MenuItem", request, cancellationToken);

    public Task UpdateItemAsync(int id, MenuItemWriteRequest request, CancellationToken cancellationToken = default) =>
        PutAsync($"api/MenuItem/{id}", request, cancellationToken);

    public Task DeleteItemAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/MenuItem/{id}", cancellationToken);
}
