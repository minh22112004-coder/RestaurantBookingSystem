using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class FoundationTests
{
    [Fact]
    public async Task HomePage_ReturnsFoundationLayoutAndAnonymousNavigation()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Table &amp; Taste", html, StringComparison.Ordinal);
        Assert.Contains("/account/login", html, StringComparison.Ordinal);
        Assert.Contains("/restaurants", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminRoute_RequiresAuthentication()
    {
        using var factory = new TestWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public void JwtSessionService_SavesReadsAndClearsAuthenticationState()
    {
        var session = new TestSession();
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        var service = new JwtSessionService(new HttpContextAccessor { HttpContext = context });
        var expected = new AuthSession(
            "signed-token",
            DateTime.UtcNow.AddHours(1),
            42,
            "customer",
            "customer@example.com",
            "Customer");

        service.Save(expected);

        Assert.Equal("signed-token", service.GetAccessToken());
        Assert.Equal(42, service.Current?.UserId);
        Assert.True(service.Current?.IsCustomer);

        service.Clear();
        Assert.Null(service.Current);
    }

    [Fact]
    public async Task ApiAuthenticationHandler_AddsBearerToken()
    {
        var recorder = new RecordingHandler();
        var handler = new ApiAuthenticationHandler(new StubJwtSessionService("jwt-token"))
        {
            InnerHandler = recorder
        };
        using var invoker = new HttpMessageInvoker(handler);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://backend.test/api/auth/me"), default);

        Assert.Equal(new AuthenticationHeaderValue("Bearer", "jwt-token"), recorder.Authorization);
    }

    [Fact]
    public async Task ApiClientBase_MapsBackendErrorMessage()
    {
        using var httpClient = new HttpClient(new JsonResponseHandler(
            HttpStatusCode.Conflict,
            "{\"message\":\"The record is in use.\"}"))
        {
            BaseAddress = new Uri("http://backend.test/")
        };
        var client = new ProbeApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<ApiClientException>(() => client.GetAsync());

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("The record is in use.", exception.Message);
    }

    [Fact]
    public async Task ApiClientBase_MapsConnectionFailureToServiceUnavailable()
    {
        using var httpClient = new HttpClient(new ConnectionFailureHandler())
        {
            BaseAddress = new Uri("http://127.0.0.1:1/")
        };
        var client = new ProbeApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<ApiClientException>(() => client.GetAsync());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
        Assert.Contains("backend API is unavailable", exception.Message, StringComparison.Ordinal);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    private sealed class ProbeApiClient : ApiClientBase
    {
        public ProbeApiClient(HttpClient httpClient) : base(httpClient) { }
        public Task<object> GetAsync() => GetAsync<object>("api/probe");
    }

    private sealed class JsonResponseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _json;

        public JsonResponseHandler(HttpStatusCode statusCode, string json)
        {
            _statusCode = statusCode;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class ConnectionFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection refused."));
    }

    private sealed class StubJwtSessionService : IJwtSessionService
    {
        private readonly string? _token;
        public StubJwtSessionService(string? token) => _token = token;
        public AuthSession? Current => null;
        public string? GetAccessToken() => _token;
        public void Save(AuthSession session) { }
        public void Clear() { }
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public required ISession Session { get; set; }
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new(StringComparer.Ordinal);
        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public IEnumerable<string> Keys => _values.Keys;
        public void Clear() => _values.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) =>
            _values.TryGetValue(key, out value);
    }
}
