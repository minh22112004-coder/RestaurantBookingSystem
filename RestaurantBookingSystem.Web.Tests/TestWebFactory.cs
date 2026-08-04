using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class TestWebFactory : WebApplicationFactory<Program>
{
    public FakeAuthApiClient AuthClient { get; } = new();
    public FakeRestaurantApiClient RestaurantClient { get; } = new();
    public FakeMenuApiClient MenuClient { get; } = new();
    public FakeDiningTableApiClient TableClient { get; } = new();
    public FakeReservationApiClient ReservationClient { get; } = new();
    public FakeNotificationApiClient NotificationClient { get; } = new();
    public FakeReportApiClient ReportClient { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackendApi:BaseUrl"] = "http://backend.test/"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthApiClient>();
            services.AddSingleton<IAuthApiClient>(AuthClient);
            services.RemoveAll<IRestaurantApiClient>();
            services.AddSingleton<IRestaurantApiClient>(RestaurantClient);
            services.RemoveAll<IMenuApiClient>();
            services.AddSingleton<IMenuApiClient>(MenuClient);
            services.RemoveAll<IDiningTableApiClient>();
            services.AddSingleton<IDiningTableApiClient>(TableClient);
            services.RemoveAll<IReservationApiClient>();
            services.AddSingleton<IReservationApiClient>(ReservationClient);
            services.RemoveAll<INotificationApiClient>();
            services.AddSingleton<INotificationApiClient>(NotificationClient);
            services.RemoveAll<IReportApiClient>();
            services.AddSingleton<IReportApiClient>(ReportClient);
        });
    }
}

public sealed class FakeAuthApiClient : IAuthApiClient
{
    public AuthResponse LoginResponse { get; set; } = CreateResponse("Customer");
    public AuthResponse RegisterResponse { get; set; } = CreateResponse("Customer");
    public ApiClientException? LoginException { get; set; }
    public ApiClientException? RegisterException { get; set; }
    public int LoginCallCount { get; private set; }
    public int RegisterCallCount { get; private set; }

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        LoginCallCount++;
        return LoginException is null
            ? Task.FromResult(LoginResponse)
            : Task.FromException<AuthResponse>(LoginException);
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        RegisterCallCount++;
        return RegisterException is null
            ? Task.FromResult(RegisterResponse)
            : Task.FromException<AuthResponse>(RegisterException);
    }

    public Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new CurrentUserDto
        {
            Id = LoginResponse.User.Id.ToString(),
            Username = LoginResponse.User.Username,
            Email = LoginResponse.User.Email,
            Roles = [LoginResponse.User.Role]
        });

    public static AuthResponse CreateResponse(string role, string username = "Minh") => new()
    {
        AccessToken = $"{role.ToLowerInvariant()}-token",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        User = new AuthenticatedUser
        {
            Id = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 1 : 42,
            Username = username,
            Email = $"{username.ToLowerInvariant()}@example.com",
            Role = role
        }
    };
}
