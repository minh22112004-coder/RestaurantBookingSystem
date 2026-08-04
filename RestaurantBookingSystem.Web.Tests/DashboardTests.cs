using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class DashboardTests
{
    [Fact]
    public async Task AdminDashboard_RendersCardsChartTableAndLoadingState()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");

        var response = await client.GetAsync("/admin");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Today's revenue", html, StringComparison.Ordinal);
        Assert.Contains("Today's reservations", html, StringComparison.Ordinal);
        Assert.Contains("Total customers", html, StringComparison.Ordinal);
        Assert.Contains("Table occupancy", html, StringComparison.Ordinal);
        Assert.Contains("Reservation trend", html, StringComparison.Ordinal);
        Assert.Contains("Seven-day summary", html, StringComparison.Ordinal);
        Assert.Contains("data-loading-state", html, StringComparison.Ordinal);
        Assert.Contains("height: 100%", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DashboardRestaurantFilter_IsForwardedToOverviewAndTrend()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");

        var response = await client.GetAsync("/admin?restaurantId=2");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Copper Kitchen", html, StringComparison.Ordinal);
        Assert.Equal(2, factory.ReportClient.LastOverviewRestaurantId);
        Assert.Equal(2, factory.ReportClient.LastReservationFilter?.RestaurantId);
        Assert.Equal("day", factory.ReportClient.LastReservationFilter?.GroupBy);
    }

    [Fact]
    public async Task DashboardWithoutTrendData_ShowsEmptyStateAndSummaryTable()
    {
        using var factory = new TestWebFactory();
        factory.ReportClient.ReservationReport = new ReservationReportDto();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");

        var response = await client.GetAsync("/admin");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No reservation activity", html, StringComparison.Ordinal);
        Assert.Contains("Seven-day summary", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DashboardApiFailure_ShowsErrorState()
    {
        using var factory = new TestWebFactory();
        factory.ReportClient.OverviewException = new ApiClientException(HttpStatusCode.InternalServerError, "Backend failed.");
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client, factory, "Admin");

        var response = await client.GetAsync("/admin");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("Dashboard data is unavailable", html, StringComparison.Ordinal);
        Assert.Contains("Try again", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "/account/login")]
    [InlineData("Customer", "/account/unauthorized")]
    public async Task Dashboard_RejectsNonAdminUsers(string? role, string expectedLocation)
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        if (role is not null)
            await LoginAsync(client, factory, role);

        var response = await client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(expectedLocation, response.Headers.Location?.OriginalString, StringComparison.Ordinal);
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
