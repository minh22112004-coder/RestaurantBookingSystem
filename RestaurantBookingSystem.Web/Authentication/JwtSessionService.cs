using System.Globalization;

namespace RestaurantBookingSystem.Web.Authentication;

public sealed class JwtSessionService : IJwtSessionService
{
    private const string TokenKey = "Auth.AccessToken";
    private const string ExpiresAtKey = "Auth.ExpiresAt";
    private const string UserIdKey = "Auth.UserId";
    private const string UsernameKey = "Auth.Username";
    private const string EmailKey = "Auth.Email";
    private const string RoleKey = "Auth.Role";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public AuthSession? Current
    {
        get
        {
            var session = GetSession();
            var token = session.GetString(TokenKey);
            var expiresValue = session.GetString(ExpiresAtKey);
            if (string.IsNullOrWhiteSpace(token) ||
                !DateTime.TryParse(expiresValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt) ||
                expiresAt <= DateTime.UtcNow ||
                !int.TryParse(session.GetString(UserIdKey), out var userId))
            {
                return null;
            }

            return new AuthSession(
                token,
                expiresAt,
                userId,
                session.GetString(UsernameKey) ?? string.Empty,
                session.GetString(EmailKey) ?? string.Empty,
                session.GetString(RoleKey) ?? string.Empty);
        }
    }

    public string? GetAccessToken() => Current?.AccessToken;

    public void Save(AuthSession authSession)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authSession.AccessToken);
        var session = GetSession();
        session.SetString(TokenKey, authSession.AccessToken);
        session.SetString(ExpiresAtKey, authSession.ExpiresAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        session.SetString(UserIdKey, authSession.UserId.ToString(CultureInfo.InvariantCulture));
        session.SetString(UsernameKey, authSession.Username);
        session.SetString(EmailKey, authSession.Email);
        session.SetString(RoleKey, authSession.Role);
    }

    public void Clear() => GetSession().Clear();

    private ISession GetSession() =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is unavailable outside an HTTP request.");
}
