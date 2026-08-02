namespace RestaurantBookingSystem.Features.Authorization.Policies;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ManagerOrAdmin = "ManagerOrAdmin";
    public const string AuthenticatedUser = "AuthenticatedUser";
}