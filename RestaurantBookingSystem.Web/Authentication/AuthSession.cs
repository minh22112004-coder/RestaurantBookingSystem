namespace RestaurantBookingSystem.Web.Authentication;

public sealed record AuthSession(
    string AccessToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string Email,
    string Role)
{
    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsCustomer => string.Equals(Role, "Customer", StringComparison.OrdinalIgnoreCase);
}
