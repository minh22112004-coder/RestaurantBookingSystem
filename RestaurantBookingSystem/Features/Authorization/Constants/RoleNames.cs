namespace RestaurantBookingSystem.Features.Authorization.Constants;

// These values must match the seeded roles in RestaurantReservationDB.sql:
// INSERT INTO [Role] (RoleName) VALUES ('Admin'), ('Manager'), ('Customer');
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Customer = "Customer";
}
