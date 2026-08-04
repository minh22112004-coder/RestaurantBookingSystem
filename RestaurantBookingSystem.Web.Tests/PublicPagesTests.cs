using System.Net;
using RestaurantBookingSystem.Web.ClientServices;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class PublicPagesTests
{
    [Fact]
    public async Task HomePage_RendersFinishedEnglishContent()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Good evenings begin with an easy reservation.", html, StringComparison.Ordinal);
        Assert.Contains("Explore restaurants", html, StringComparison.Ordinal);
        Assert.Contains("Reserve with ease", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantList_RendersRestaurantsAndOpeningHours()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Lotus House", html, StringComparison.Ordinal);
        Assert.Contains("Copper Kitchen", html, StringComparison.Ordinal);
        Assert.Contains("08:00 - 22:00", html, StringComparison.Ordinal);
        Assert.Contains("2 restaurants", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantList_SearchFiltersByNameOrAddress()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants?search=market");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Copper Kitchen", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Lotus House", html, StringComparison.Ordinal);
        Assert.Contains("1 restaurant", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantList_EmptyCollectionRendersEmptyState()
    {
        using var factory = new TestWebFactory();
        factory.RestaurantClient.Restaurants.Clear();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("There is no restaurant to show yet.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantList_BackendFailureRendersEnglishErrorState()
    {
        using var factory = new TestWebFactory();
        factory.RestaurantClient.GetAllException = new ApiClientException(
            HttpStatusCode.InternalServerError,
            "Backend-specific message.");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("The restaurant directory is temporarily unavailable.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Backend-specific message", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantDetails_RendersInformationMenuAndTables()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants/1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Lotus House", html, StringComparison.Ordinal);
        Assert.Contains("Opening hours", html, StringComparison.Ordinal);
        Assert.Contains("Lotus salad", html, StringComparison.Ordinal);
        Assert.Contains("85,000 VND", html, StringComparison.Ordinal);
        Assert.Contains("Table T01", html, StringComparison.Ordinal);
        Assert.Contains("Seats up to <strong>10</strong> guests", html, StringComparison.Ordinal);
        Assert.Contains("Currently unavailable", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantDetails_MissingRestaurantReturnsNotFoundState()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants/999");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("We could not open this restaurant.", html, StringComparison.Ordinal);
        Assert.Contains("We could not find that restaurant.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RestaurantDetails_ChildDataFailureKeepsRestaurantContext()
    {
        using var factory = new TestWebFactory();
        factory.MenuClient.GetItemsException = new ApiClientException(
            HttpStatusCode.ServiceUnavailable,
            "Backend-specific message.");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/restaurants/1");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("Lotus House", html, StringComparison.Ordinal);
        Assert.Contains("Some details are unavailable", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Backend-specific message", html, StringComparison.Ordinal);
    }
}
