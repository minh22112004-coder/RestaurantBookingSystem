namespace RestaurantBookingSystem.Web.Authentication;

public interface IJwtSessionService
{
    AuthSession? Current { get; }
    string? GetAccessToken();
    void Save(AuthSession session);
    void Clear();
}
