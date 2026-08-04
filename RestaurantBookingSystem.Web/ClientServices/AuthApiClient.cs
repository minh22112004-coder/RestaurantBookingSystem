using RestaurantBookingSystem.Web.Contracts;

namespace RestaurantBookingSystem.Web.ClientServices;

public interface IAuthApiClient
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthApiClient : ApiClientBase, IAuthApiClient
{
    public AuthApiClient(HttpClient httpClient) : base(httpClient) { }

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginRequest, AuthResponse>("api/auth/login", request, cancellationToken);

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<RegisterRequest, AuthResponse>("api/auth/register", request, cancellationToken);

    public Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetAsync<CurrentUserDto>("api/auth/me", cancellationToken);
}
