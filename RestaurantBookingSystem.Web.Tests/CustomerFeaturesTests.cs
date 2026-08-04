using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class CustomerFeaturesTests
{
    [Fact]
    public async Task AnonymousRestaurantDetails_ShowsSignInGate()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants/1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sign in to request this table.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Submit reservation request", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomerRestaurantDetails_RendersBookingFormWithoutUserId()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/restaurants/1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Submit reservation request", html, StringComparison.Ordinal);
        Assert.Contains("name=\"BookingForm.TableId\"", html, StringComparison.Ordinal);
        Assert.Contains("data-capacity=\"2\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"UserId\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidBooking_SubmitsJwtOwnedRequestAndRedirectsToReservations()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/restaurants/1");
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(3));

        var response = await client.PostAsync("/restaurants/1/reservations", Form(
            token,
            ("BookingForm.TableId", "1"),
            ("BookingForm.Date", date.ToString("yyyy-MM-dd")),
            ("BookingForm.StartTime", "18:00"),
            ("BookingForm.EndTime", "20:00"),
            ("BookingForm.GuestCount", "2")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/reservations", response.Headers.Location?.OriginalString);
        Assert.NotNull(factory.ReservationClient.LastCreateRequest);
        Assert.Equal(1, factory.ReservationClient.LastCreateRequest.TableId);
        Assert.Equal(2, factory.ReservationClient.LastCreateRequest.GuestCount);
    }

    [Fact]
    public async Task BookingAboveTableCapacity_IsRejectedBeforeApiCall()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/restaurants/1");

        var response = await client.PostAsync("/restaurants/1/reservations", Form(
            token,
            ("BookingForm.TableId", "1"),
            ("BookingForm.Date", DateOnly.FromDateTime(DateTime.Today.AddDays(3)).ToString("yyyy-MM-dd")),
            ("BookingForm.StartTime", "18:00"),
            ("BookingForm.EndTime", "20:00"),
            ("BookingForm.GuestCount", "4")));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(factory.ReservationClient.LastCreateRequest);
        Assert.Contains("This table seats a maximum of 2 guests.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BookingWithPastDateAndInvalidTime_IsRejectedBeforeApiCall()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/restaurants/1");

        var response = await client.PostAsync("/restaurants/1/reservations", Form(
            token,
            ("BookingForm.TableId", "1"),
            ("BookingForm.Date", DateOnly.FromDateTime(DateTime.Today.AddDays(-1)).ToString("yyyy-MM-dd")),
            ("BookingForm.StartTime", "20:00"),
            ("BookingForm.EndTime", "18:00"),
            ("BookingForm.GuestCount", "2")));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(factory.ReservationClient.LastCreateRequest);
        Assert.Contains("Reservations cannot be made in the past.", html, StringComparison.Ordinal);
        Assert.Contains("The end time must be later than the start time.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MyReservations_RendersActiveAndCancelledReservations()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/reservations");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Reservation #10", html, StringComparison.Ordinal);
        Assert.Contains("Edit reservation", html, StringComparison.Ordinal);
        Assert.Contains("Cancelled", html, StringComparison.Ordinal);
        Assert.Contains("data-confirm-form", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CancelledReservation_CannotOpenEditPage()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/reservations/11/edit");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Cancelled reservations cannot be updated.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidReservationEdit_UpdatesApiAndRedirects()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/reservations/10/edit");

        var response = await client.PostAsync("/reservations/10/edit", Form(
            token,
            ("Form.TableId", "3"),
            ("Form.Date", DateOnly.FromDateTime(DateTime.Today.AddDays(5)).ToString("yyyy-MM-dd")),
            ("Form.StartTime", "17:00"),
            ("Form.EndTime", "19:00"),
            ("Form.GuestCount", "6")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/reservations", response.Headers.Location?.OriginalString);
        Assert.Equal(10, factory.ReservationClient.LastUpdatedId);
        Assert.Equal(3, factory.ReservationClient.LastUpdateRequest?.TableId);
    }

    [Fact]
    public async Task CancelReservation_CallsApiAndRedirects()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/reservations");

        var response = await client.PostAsync("/reservations/10/cancel", Form(token));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(10, factory.ReservationClient.LastCancelledId);
        Assert.Equal("Cancelled", factory.ReservationClient.Reservations.Single(item => item.ReservationId == 10).Status);
    }

    [Fact]
    public async Task CustomerHeader_ShowsUnreadNotificationCount()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/account/profile");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("notification-badge", html, StringComparison.Ordinal);
        Assert.Contains("1 unread notifications", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NotificationsPage_RendersReadAndUnreadItems()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/notifications");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Reservation received", html, StringComparison.Ordinal);
        Assert.Contains("Reservation updated", html, StringComparison.Ordinal);
        Assert.Contains("Mark as read", html, StringComparison.Ordinal);
        Assert.Contains("<strong>1</strong><span>unread</span>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MarkNotificationAsRead_CallsApiAndRedirects()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/notifications");

        var response = await client.PostAsync("/notifications/1/read", Form(token));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(1, factory.NotificationClient.LastReadId);
        Assert.True(factory.NotificationClient.Notifications.Single(item => item.NotificationId == 1).IsRead);
    }

    private static HttpClient CreateNoRedirectClient(TestWebFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task LoginAsync(HttpClient client)
    {
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");
        var response = await client.PostAsync("/account/login", Form(
            token,
            ("Email", "minh@example.com"),
            ("Password", "secret123")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);
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
