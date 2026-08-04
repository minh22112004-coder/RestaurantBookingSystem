using System.Net.Http.Headers;

namespace RestaurantBookingSystem.Web.Authentication;

public sealed class ApiAuthenticationHandler : DelegatingHandler
{
    private readonly IJwtSessionService _sessionService;

    public ApiAuthenticationHandler(IJwtSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _sessionService.GetAccessToken();
        if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization is null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
