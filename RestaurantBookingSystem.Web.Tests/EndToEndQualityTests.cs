using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class EndToEndQualityTests
{
    [Fact]
    public async Task CustomerJourney_LoginCreateUpdateAndCancelReservation()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Customer");
        var bookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(4));
        var token = await GetAntiForgeryTokenAsync(client, "/restaurants/1");

        var create = await client.PostAsync("/restaurants/1/reservations", Form(token,
            ("BookingForm.TableId", "1"), ("BookingForm.Date", bookingDate.ToString("yyyy-MM-dd")),
            ("BookingForm.StartTime", "18:00"), ("BookingForm.EndTime", "20:00"),
            ("BookingForm.GuestCount", "2")));
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);
        var reservation = factory.ReservationClient.Reservations.MaxBy(item => item.ReservationId)!;

        token = await GetAntiForgeryTokenAsync(client, $"/reservations/{reservation.ReservationId}/edit");
        var update = await client.PostAsync($"/reservations/{reservation.ReservationId}/edit", Form(token,
            ("Form.TableId", "3"), ("Form.Date", bookingDate.AddDays(1).ToString("yyyy-MM-dd")),
            ("Form.StartTime", "17:00"), ("Form.EndTime", "19:00"), ("Form.GuestCount", "6")));
        Assert.Equal(HttpStatusCode.Redirect, update.StatusCode);
        Assert.Equal(3, reservation.TableId);

        token = await GetAntiForgeryTokenAsync(client, "/reservations");
        var cancel = await client.PostAsync($"/reservations/{reservation.ReservationId}/cancel", Form(token));

        Assert.Equal(HttpStatusCode.Redirect, cancel.StatusCode);
        Assert.Equal("Cancelled", reservation.Status);
    }

    [Fact]
    public async Task AdminJourney_LoginAndCreateRestaurantTableCategoryAndMenuItem()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");
        var token = await GetAntiForgeryTokenAsync(client, "/admin/restaurants");

        var restaurantResponse = await client.PostAsync("/admin/restaurants/create", Form(token,
            ("Form.Name", "Quality Bistro"), ("Form.Address", "10 Quality Avenue"),
            ("Form.Phone", "02822223333"), ("Form.OpenTime", "08:00"), ("Form.CloseTime", "22:00")));
        Assert.Equal(HttpStatusCode.Redirect, restaurantResponse.StatusCode);
        var restaurant = factory.RestaurantClient.Restaurants.Single(item => item.Name == "Quality Bistro");

        token = await GetAntiForgeryTokenAsync(client, $"/admin/tables?restaurantId={restaurant.RestaurantId}");
        var tableResponse = await client.PostAsync("/admin/tables/create", Form(token,
            ("Form.RestaurantId", restaurant.RestaurantId.ToString()), ("Form.TableNumber", "Q01"),
            ("Form.Capacity", "4"), ("Form.Status", "Available")));
        Assert.Equal(HttpStatusCode.Redirect, tableResponse.StatusCode);

        token = await GetAntiForgeryTokenAsync(client, "/admin/menu");
        var categoryResponse = await client.PostAsync("/admin/menu/categories/create", Form(token,
            ("CategoryForm.Name", "Quality specials")));
        Assert.Equal(HttpStatusCode.Redirect, categoryResponse.StatusCode);
        var category = factory.MenuClient.Categories.Single(item => item.Name == "Quality specials");

        token = await GetAntiForgeryTokenAsync(client, $"/admin/menu?restaurantId={restaurant.RestaurantId}");
        var itemResponse = await client.PostAsync("/admin/menu/items/create", Form(token,
            ("ItemForm.RestaurantId", restaurant.RestaurantId.ToString()),
            ("ItemForm.CategoryId", category.CategoryId.ToString()),
            ("ItemForm.Name", "Quality plate"), ("ItemForm.Price", "120000"),
            ("ItemForm.Available", "true")));

        Assert.Equal(HttpStatusCode.Redirect, itemResponse.StatusCode);
        Assert.Contains(factory.TableClient.Tables, item => item.TableNumber == "Q01" && item.RestaurantId == restaurant.RestaurantId);
        Assert.Contains(factory.MenuClient.Items, item => item.Name == "Quality plate" && item.RestaurantId == restaurant.RestaurantId);
    }

    [Fact]
    public async Task AuthenticatedHtml_DoesNotExposeJwtAndSessionCookieIsHttpOnly()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.LoginResponse = FakeAuthApiClient.CreateResponse("Admin", "SecurityAdmin");
        using var client = CreateNoRedirectClient(factory);
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");

        var login = await client.PostAsync("/account/login", Form(token,
            ("Email", "admin@example.com"), ("Password", "secret123")));
        var dashboard = await client.GetAsync("/admin");
        var html = await dashboard.Content.ReadAsStringAsync();
        var cookies = string.Join(';', login.Headers.GetValues("Set-Cookie"));

        Assert.DoesNotContain(factory.AuthClient.LoginResponse.AccessToken, html, StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization: Bearer", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".RestaurantBookingSystem.Web.Session", cookies, StringComparison.Ordinal);
        Assert.Contains("httponly", cookies, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderedPages_ExposeKeyboardAndDialogAccessibilityContracts()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);

        var publicHtml = await client.GetStringAsync("/restaurants");
        Assert.Single(Regex.Matches(publicHtml, "<main[\\s>]", RegexOptions.IgnoreCase).Cast<Match>());
        Assert.Contains("Skip to main content", publicHtml, StringComparison.Ordinal);
        Assert.Contains("role=\"search\"", publicHtml, StringComparison.Ordinal);

        await LoginAsync(client, factory, "Customer");
        var reservationsHtml = await client.GetStringAsync("/reservations");
        Assert.Contains("aria-labelledby=\"confirm-dialog-title\"", reservationsHtml, StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"confirm-dialog-message\"", reservationsHtml, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"page\"", reservationsHtml, StringComparison.Ordinal);
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
