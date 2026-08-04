using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantBookingSystem.Web.ClientServices;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task LoginPage_RendersAuthenticationForm()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/account/login");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sign in", html, StringComparison.Ordinal);
        Assert.Contains("name=\"__RequestVerificationToken\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Email\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidLogin_DoesNotCallBackendAndShowsValidationMessage()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");

        var response = await client.PostAsync("/account/login", Form(
            token,
            ("Email", ""),
            ("Password", "")));
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, factory.AuthClient.LoginCallCount);
        Assert.Contains("Please enter your email or username", html, StringComparison.Ordinal);
        Assert.Contains("Please enter your password", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BackendRejectsLogin_ShowsApiMessageWithoutCreatingSession()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.LoginException = new ApiClientException(
            HttpStatusCode.Unauthorized,
            "Backend authentication failure.");
        using var client = factory.CreateClient();
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");

        var response = await client.PostAsync("/account/login", Form(
            token,
            ("Email", "minh@example.com"),
            ("Password", "wrong-password")));
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, factory.AuthClient.LoginCallCount);
        Assert.Contains("Email, username, or password is incorrect.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomerLogin_PersistsSessionAndUnlocksCustomerPages()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.LoginResponse = FakeAuthApiClient.CreateResponse("Customer", "Lan");
        using var client = CreateNoRedirectClient(factory);
        var token = await GetAntiForgeryTokenAsync(client, "/account/login?returnUrl=%2Faccount%2Fprofile");

        var loginResponse = await client.PostAsync("/account/login", Form(
            token,
            ("Email", "lan@example.com"),
            ("Password", "secret123"),
            ("ReturnUrl", "/account/profile")));
        var profileResponse = await client.GetAsync("/account/profile");
        var profileHtml = WebUtility.HtmlDecode(await profileResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.Equal("/account/profile", loginResponse.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        Assert.Contains("Lan", profileHtml, StringComparison.Ordinal);
        Assert.Contains("lan@example.com", profileHtml, StringComparison.Ordinal);
        Assert.Contains("/reservations", profileHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminLogin_RedirectsToAdminAndRendersAdminLayout()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.LoginResponse = FakeAuthApiClient.CreateResponse("Admin", "Administrator");
        using var client = CreateNoRedirectClient(factory);
        var token = await GetAntiForgeryTokenAsync(client, "/account/login");

        var loginResponse = await client.PostAsync("/account/login", Form(
            token,
            ("Email", "admin"),
            ("Password", "123456")));
        var adminResponse = await client.GetAsync("/admin");
        var html = await adminResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.Equal("/Admin", loginResponse.Headers.Location?.OriginalString, ignoreCase: true);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Contains("admin-sidebar", html, StringComparison.Ordinal);
        Assert.Contains("Administrator", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomerCannotAccessAdminArea()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);

        var response = await client.GetAsync("/admin");
        var forbiddenResponse = await client.GetAsync(response.Headers.Location!);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/account/unauthorized", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Register_CreatesAuthenticatedCustomerSession()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.RegisterResponse = FakeAuthApiClient.CreateResponse("Customer", "Bao");
        using var client = CreateNoRedirectClient(factory);
        var token = await GetAntiForgeryTokenAsync(client, "/account/register");

        var response = await client.PostAsync("/account/register", Form(
            token,
            ("Username", "Bao"),
            ("Email", "bao@example.com"),
            ("Phone", "0901234567"),
            ("Password", "secret123"),
            ("ConfirmPassword", "secret123")));
        var profileResponse = await client.GetAsync("/account/profile");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(1, factory.AuthClient.RegisterCallCount);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
    }

    [Fact]
    public async Task BackendUnavailableDuringRegistration_ShowsFormErrorInsteadOfCrashing()
    {
        using var factory = new TestWebFactory();
        factory.AuthClient.RegisterException = new ApiClientException(
            HttpStatusCode.ServiceUnavailable,
            "The backend API is unavailable.");
        using var client = CreateNoRedirectClient(factory);
        var token = await GetAntiForgeryTokenAsync(client, "/account/register");

        var response = await client.PostAsync("/account/register", Form(
            token,
            ("Username", "Bao"),
            ("Email", "bao@example.com"),
            ("Phone", "0901234567"),
            ("Password", "secret123"),
            ("ConfirmPassword", "secret123")));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("The backend API is not running", html, StringComparison.Ordinal);
        Assert.Equal(1, factory.AuthClient.RegisterCallCount);
    }

    [Fact]
    public async Task Logout_ClearsSessionAndProtectedPageRedirectsToLogin()
    {
        using var factory = new TestWebFactory();
        using var client = CreateNoRedirectClient(factory);
        await LoginAsync(client);
        var token = await GetAntiForgeryTokenAsync(client, "/account/profile");

        var logoutResponse = await client.PostAsync("/account/logout", Form(token));
        var protectedResponse = await client.GetAsync("/account/profile");

        Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);
        Assert.Equal("/", logoutResponse.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.StartsWith("/account/login", protectedResponse.Headers.Location?.OriginalString, StringComparison.Ordinal);
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
