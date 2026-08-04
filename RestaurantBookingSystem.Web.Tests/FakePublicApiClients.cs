using System.Net;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class FakeRestaurantApiClient : IRestaurantApiClient
{
    public List<RestaurantDto> Restaurants { get; } =
    [
        new()
        {
            RestaurantId = 1,
            Name = "Lotus House",
            Address = "123 Riverside Avenue",
            Phone = "0283456789",
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(22, 0)
        },
        new()
        {
            RestaurantId = 2,
            Name = "Copper Kitchen",
            Address = "45 Market Street",
            Phone = "0289876543",
            OpenTime = new TimeOnly(10, 30),
            CloseTime = new TimeOnly(23, 0)
        }
    ];

    public ApiClientException? GetAllException { get; set; }
    public ApiClientException? GetByIdException { get; set; }
    public ApiClientException? CreateException { get; set; }
    public ApiClientException? UpdateException { get; set; }
    public ApiClientException? DeleteException { get; set; }
    public RestaurantWriteRequest? LastCreateRequest { get; private set; }
    public RestaurantWriteRequest? LastUpdateRequest { get; private set; }
    public int? LastUpdatedId { get; private set; }
    public int? LastDeletedId { get; private set; }

    public Task<List<RestaurantDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllException is null
            ? Task.FromResult(Restaurants.ToList())
            : Task.FromException<List<RestaurantDto>>(GetAllException);

    public Task<RestaurantDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (GetByIdException is not null)
            return Task.FromException<RestaurantDto>(GetByIdException);

        var restaurant = Restaurants.FirstOrDefault(item => item.RestaurantId == id);
        return restaurant is null
            ? Task.FromException<RestaurantDto>(new ApiClientException(HttpStatusCode.NotFound, "Not found."))
            : Task.FromResult(restaurant);
    }

    public Task<RestaurantDto> CreateAsync(RestaurantWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (CreateException is not null) return Task.FromException<RestaurantDto>(CreateException);
        LastCreateRequest = request;
        var item = new RestaurantDto { RestaurantId = Restaurants.Max(x => x.RestaurantId) + 1, Name = request.Name, Address = request.Address, Phone = request.Phone, OpenTime = request.OpenTime, CloseTime = request.CloseTime };
        Restaurants.Add(item);
        return Task.FromResult(item);
    }
    public Task UpdateAsync(int id, RestaurantWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (UpdateException is not null) return Task.FromException(UpdateException);
        var item = Restaurants.FirstOrDefault(x => x.RestaurantId == id);
        if (item is null) return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        LastUpdatedId = id; LastUpdateRequest = request;
        item.Name = request.Name; item.Address = request.Address; item.Phone = request.Phone; item.OpenTime = request.OpenTime; item.CloseTime = request.CloseTime;
        return Task.CompletedTask;
    }
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (DeleteException is not null) return Task.FromException(DeleteException);
        LastDeletedId = id;
        Restaurants.RemoveAll(x => x.RestaurantId == id);
        return Task.CompletedTask;
    }
}

public sealed class FakeMenuApiClient : IMenuApiClient
{
    public List<CategoryDto> Categories { get; } =
    [
        new() { CategoryId = 1, Name = "Starters" },
        new() { CategoryId = 2, Name = "Mains" },
        new() { CategoryId = 3, Name = "Drinks" }
    ];
    public List<MenuItemDto> Items { get; } =
    [
        new() { MenuItemId = 1, RestaurantId = 1, RestaurantName = "Lotus House", CategoryId = 1, CategoryName = "Starters", Name = "Lotus salad", Price = 85000, Available = true },
        new() { MenuItemId = 2, RestaurantId = 1, RestaurantName = "Lotus House", CategoryId = 2, CategoryName = "Mains", Name = "Shaking beef", Price = 150000, Available = true },
        new() { MenuItemId = 3, RestaurantId = 1, RestaurantName = "Lotus House", CategoryId = 3, CategoryName = "Drinks", Name = "Iced tea", Price = 10000, Available = false }
    ];

    public ApiClientException? GetItemsException { get; set; }
    public ApiClientException? DeleteCategoryException { get; set; }
    public ApiClientException? DeleteItemException { get; set; }
    public int? LastUpdatedCategoryId { get; private set; }
    public int? LastDeletedCategoryId { get; private set; }
    public int? LastUpdatedItemId { get; private set; }
    public int? LastDeletedItemId { get; private set; }

    public Task<List<MenuItemDto>> GetItemsAsync(int? restaurantId = null, CancellationToken cancellationToken = default) =>
        GetItemsException is null
            ? Task.FromResult(Items.Where(item => !restaurantId.HasValue || item.RestaurantId == restaurantId).ToList())
            : Task.FromException<List<MenuItemDto>>(GetItemsException);

    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Categories.ToList());
    public Task<CategoryDto> CreateCategoryAsync(CategoryWriteRequest request, CancellationToken cancellationToken = default)
    {
        var item = new CategoryDto { CategoryId = Categories.Max(x => x.CategoryId) + 1, Name = request.Name };
        Categories.Add(item); return Task.FromResult(item);
    }
    public Task UpdateCategoryAsync(int id, CategoryWriteRequest request, CancellationToken cancellationToken = default)
    {
        var item = Categories.FirstOrDefault(x => x.CategoryId == id);
        if (item is null) return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        LastUpdatedCategoryId = id; item.Name = request.Name; return Task.CompletedTask;
    }
    public Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        if (DeleteCategoryException is not null) return Task.FromException(DeleteCategoryException);
        LastDeletedCategoryId = id; Categories.RemoveAll(x => x.CategoryId == id); return Task.CompletedTask;
    }
    public Task<MenuItemDto> CreateItemAsync(MenuItemWriteRequest request, CancellationToken cancellationToken = default)
    {
        var item = new MenuItemDto { MenuItemId = Items.Max(x => x.MenuItemId) + 1, RestaurantId = request.RestaurantId, RestaurantName = "Restaurant", CategoryId = request.CategoryId, CategoryName = Categories.FirstOrDefault(x => x.CategoryId == request.CategoryId)?.Name ?? "Category", Name = request.Name, Price = request.Price, Available = request.Available };
        Items.Add(item); return Task.FromResult(item);
    }
    public Task UpdateItemAsync(int id, MenuItemWriteRequest request, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(x => x.MenuItemId == id);
        if (item is null) return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        LastUpdatedItemId = id; item.RestaurantId = request.RestaurantId; item.CategoryId = request.CategoryId; item.Name = request.Name; item.Price = request.Price; item.Available = request.Available; return Task.CompletedTask;
    }
    public Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
    {
        if (DeleteItemException is not null) return Task.FromException(DeleteItemException);
        LastDeletedItemId = id; Items.RemoveAll(x => x.MenuItemId == id); return Task.CompletedTask;
    }
}

public sealed class FakeDiningTableApiClient : IDiningTableApiClient
{
    public List<DiningTableDto> Tables { get; } =
    [
        new() { TableId = 1, RestaurantId = 1, TableNumber = "T01", Capacity = 2, Status = "Available" },
        new() { TableId = 2, RestaurantId = 1, TableNumber = "T02", Capacity = 4, Status = "Occupied" },
        new() { TableId = 3, RestaurantId = 1, TableNumber = "V01", Capacity = 10, Status = "Available" }
    ];

    public ApiClientException? GetByRestaurantException { get; set; }
    public ApiClientException? DeleteException { get; set; }
    public DiningTableWriteRequest? LastCreateRequest { get; private set; }
    public DiningTableWriteRequest? LastUpdateRequest { get; private set; }
    public int? LastUpdatedId { get; private set; }
    public int? LastDeletedId { get; private set; }

    public Task<List<DiningTableDto>> GetByRestaurantAsync(int restaurantId, CancellationToken cancellationToken = default) =>
        GetByRestaurantException is null
            ? Task.FromResult(Tables.Where(table => table.RestaurantId == restaurantId).ToList())
            : Task.FromException<List<DiningTableDto>>(GetByRestaurantException);

    public Task<List<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Tables.ToList());
    public Task<DiningTableDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var table = Tables.FirstOrDefault(item => item.TableId == id);
        return table is null
            ? Task.FromException<DiningTableDto>(new ApiClientException(HttpStatusCode.NotFound, "Not found."))
            : Task.FromResult(table);
    }
    public Task<DiningTableDto> CreateAsync(DiningTableWriteRequest request, CancellationToken cancellationToken = default)
    {
        LastCreateRequest = request;
        var item = new DiningTableDto { TableId = Tables.Max(x => x.TableId) + 1, RestaurantId = request.RestaurantId, TableNumber = request.TableNumber, Capacity = request.Capacity, Status = request.Status };
        Tables.Add(item); return Task.FromResult(item);
    }
    public Task UpdateAsync(int id, DiningTableWriteRequest request, CancellationToken cancellationToken = default)
    {
        var item = Tables.FirstOrDefault(x => x.TableId == id);
        if (item is null) return Task.FromException(new ApiClientException(HttpStatusCode.NotFound, "Not found."));
        LastUpdatedId = id; LastUpdateRequest = request; item.RestaurantId = request.RestaurantId; item.TableNumber = request.TableNumber; item.Capacity = request.Capacity; item.Status = request.Status; return Task.CompletedTask;
    }
    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (DeleteException is not null) return Task.FromException(DeleteException);
        LastDeletedId = id; Tables.RemoveAll(x => x.TableId == id); return Task.CompletedTask;
    }
}
