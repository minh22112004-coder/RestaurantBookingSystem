using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantBookingSystem.Web.Areas.Admin.Controllers;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class AdminManagementTests
{
    [Fact]
    public async Task AnonymousAdminPage_RedirectsToLogin()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);

        var response = await client.GetAsync("/admin/restaurants");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomerAdminPage_RedirectsToUnauthorizedPage()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Customer");

        var response = await client.GetAsync("/admin/tables");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/account/unauthorized", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/admin/restaurants", "Restaurant list")]
    [InlineData("/admin/tables", "Table list")]
    [InlineData("/admin/menu", "Menu items")]
    [InlineData("/admin/reservations", "Reservation management")]
    public async Task AdminManagementPages_RenderSuccessfully(string path, string expectedText)
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");

        var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedText, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateRestaurant_ValidFormCallsApi()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var token = await GetAntiForgeryTokenAsync(client, "/admin/restaurants");

        var response = await client.PostAsync("/admin/restaurants/create", Form(token,
            ("Form.Name", "Harbor Grill"), ("Form.Address", "88 Harbor Road"),
            ("Form.Phone", "02811112222"), ("Form.OpenTime", "09:00"), ("Form.CloseTime", "22:30")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("Harbor Grill", factory.RestaurantClient.LastCreateRequest?.Name);
        Assert.Contains(factory.RestaurantClient.Restaurants, item => item.Name == "Harbor Grill");
    }

    [Fact]
    public async Task DeleteRestaurantConflict_ShowsActionableError()
    {
        using var factory = new TestWebFactory();
        factory.RestaurantClient.DeleteException = new ApiClientException(HttpStatusCode.Conflict, "In use.");
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var token = await GetAntiForgeryTokenAsync(client, "/admin/restaurants");

        var response = await client.PostAsync("/admin/restaurants/1/delete", Form(token));
        var page = await client.GetAsync(response.Headers.Location!);
        var html = await page.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("cannot be deleted because related data is still in use", html, StringComparison.Ordinal);
        Assert.Null(factory.RestaurantClient.LastDeletedId);
    }

    [Fact]
    public async Task CreateAndEditTable_CallApiWithSelectedRestaurant()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var token = await GetAntiForgeryTokenAsync(client, "/admin/tables");

        var createResponse = await client.PostAsync("/admin/tables/create", Form(token,
            ("Form.RestaurantId", "2"), ("Form.TableNumber", "P01"),
            ("Form.Capacity", "6"), ("Form.Status", "Available")));

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        Assert.Equal(2, factory.TableClient.LastCreateRequest?.RestaurantId);
        var created = factory.TableClient.Tables.Single(item => item.TableNumber == "P01");

        token = await GetAntiForgeryTokenAsync(client, $"/admin/tables/{created.TableId}/edit");
        var editResponse = await client.PostAsync($"/admin/tables/{created.TableId}/edit", Form(token,
            ("Form.RestaurantId", "2"), ("Form.TableNumber", "P02"),
            ("Form.Capacity", "8"), ("Form.Status", "Maintenance")));

        Assert.Equal(HttpStatusCode.Redirect, editResponse.StatusCode);
        Assert.Equal(created.TableId, factory.TableClient.LastUpdatedId);
        Assert.Equal("P02", factory.TableClient.LastUpdateRequest?.TableNumber);
    }

    [Fact]
    public async Task CategoryAndMenuItemCrud_CallApiClients()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var token = await GetAntiForgeryTokenAsync(client, "/admin/menu");

        var categoryResponse = await client.PostAsync("/admin/menu/categories/create", Form(token,
            ("CategoryForm.Name", "Desserts")));
        Assert.Equal(HttpStatusCode.Redirect, categoryResponse.StatusCode);
        var category = factory.MenuClient.Categories.Single(item => item.Name == "Desserts");

        token = await GetAntiForgeryTokenAsync(client, $"/admin/menu/categories/{category.CategoryId}/edit");
        var categoryEdit = await client.PostAsync($"/admin/menu/categories/{category.CategoryId}/edit", Form(token,
            ("Form.Name", "Sweet courses")));
        Assert.Equal(HttpStatusCode.Redirect, categoryEdit.StatusCode);
        Assert.Equal(category.CategoryId, factory.MenuClient.LastUpdatedCategoryId);

        token = await GetAntiForgeryTokenAsync(client, "/admin/menu");
        var itemResponse = await client.PostAsync("/admin/menu/items/create", Form(token,
            ("ItemForm.RestaurantId", "1"), ("ItemForm.CategoryId", category.CategoryId.ToString()),
            ("ItemForm.Name", "Cheesecake"), ("ItemForm.Price", "95000"), ("ItemForm.Available", "true")));

        Assert.Equal(HttpStatusCode.Redirect, itemResponse.StatusCode);
        var menuItem = factory.MenuClient.Items.Single(item => item.Name == "Cheesecake" && item.CategoryId == category.CategoryId);

        token = await GetAntiForgeryTokenAsync(client, $"/admin/menu/items/{menuItem.MenuItemId}/edit");
        var itemEdit = await client.PostAsync($"/admin/menu/items/{menuItem.MenuItemId}/edit", Form(token,
            ("Form.RestaurantId", "1"), ("Form.CategoryId", category.CategoryId.ToString()),
            ("Form.Name", "Baked cheesecake"), ("Form.Price", "99000"), ("Form.Available", "true")));
        Assert.Equal(HttpStatusCode.Redirect, itemEdit.StatusCode);
        Assert.Equal(menuItem.MenuItemId, factory.MenuClient.LastUpdatedItemId);
    }

    [Fact]
    public async Task ReservationFiltersAndCancel_AreApplied()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        var filtered = await client.GetAsync($"/admin/reservations?date={date:yyyy-MM-dd}&restaurantId=1&status=Pending");
        var html = await filtered.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        Assert.Contains("Lotus House", html, StringComparison.Ordinal);
        Assert.Contains("18:00", html, StringComparison.Ordinal);

        var empty = await client.GetStringAsync($"/admin/reservations?date={date:yyyy-MM-dd}&restaurantId=1&status=Confirmed");
        Assert.Contains("No reservations match the selected filters.", empty, StringComparison.Ordinal);

        var token = await GetAntiForgeryTokenAsync(client, $"/admin/reservations/10/edit?date={date:yyyy-MM-dd}");
        var edit = await client.PostAsync("/admin/reservations/10/edit", Form(token,
            ("userId", "42"), ("status", "Pending"), ("Form.TableId", "3"),
            ("Form.Date", date.AddDays(1).ToString("yyyy-MM-dd")), ("Form.StartTime", "17:00"),
            ("Form.EndTime", "19:00"), ("Form.GuestCount", "6")));
        Assert.Equal(HttpStatusCode.Redirect, edit.StatusCode);
        Assert.Equal(10, factory.ReservationClient.LastUpdatedId);

        token = await GetAntiForgeryTokenAsync(client, $"/admin/reservations?date={date.AddDays(1):yyyy-MM-dd}");
        var cancel = await client.PostAsync($"/admin/reservations/10/cancel?date={date:yyyy-MM-dd}", Form(token));
        Assert.Equal(HttpStatusCode.Redirect, cancel.StatusCode);
        Assert.Equal(10, factory.ReservationClient.LastCancelledId);
    }

    [Fact]
    public void EveryAdminController_RequiresAdminSessionRole()
    {
        var controllerTypes = typeof(DashboardController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
            .Where(type => type.GetCustomAttribute<AreaAttribute>()?.RouteValue == "Admin")
            .ToList();

        Assert.NotEmpty(controllerTypes);
        Assert.All(controllerTypes, type => Assert.NotNull(type.GetCustomAttribute<RequireSessionRoleAttribute>()));
    }

    private static HttpClient CreateNoRedirectClient(TestWebFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task LoginAsync(HttpClient client, TestWebFactory factory, string role)
    {
        factory.AuthClient.LoginResponse = FakeAuthApiClient.CreateResponse(role, role == "Admin" ? "Administrator" : "Customer");
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");
        var response = await client.PostAsync("/account/login", Form(token,
            ("Email", "user@example.com"), ("Password", "secret123")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.CultureInvariant);
        Assert.True(match.Success, "The form does not contain an anti-forgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent Form(string token, params (string Name, string Value)[] fields)
    {
        var values = fields.ToDictionary(field => field.Name, field => field.Value);
        values["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(values);
    }
}
