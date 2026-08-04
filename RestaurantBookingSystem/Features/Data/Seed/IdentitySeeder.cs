using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Data.Seed;

public static class IdentitySeeder
{
    public const string DemoAdminUsername = "admin";
    public const string DemoAdminEmail = "admin@restaurant.demo";
    public const string DemoAdminPassword = "123456";

    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        RestaurantReservationDbContext context =
            serviceProvider.GetRequiredService<RestaurantReservationDbContext>();

        string[] roles =
        {
            RoleNames.Admin,
            RoleNames.Manager,
            RoleNames.Customer
        };

        foreach (string roleName in roles)
        {
            bool roleExists = await context.Roles
                .AnyAsync(r => r.RoleName == roleName);

            if (roleExists)
            {
                continue;
            }

            context.Roles.Add(new Role { RoleName = roleName });
        }

        await context.SaveChangesAsync();

        Role adminRole = await context.Roles
            .SingleAsync(role => role.RoleName == RoleNames.Admin);

        User? admin = await context.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user =>
                user.Username == DemoAdminUsername ||
                user.Email == DemoAdminEmail);

        var passwordHasher = new PasswordHasher<User>();

        if (admin is null)
        {
            admin = new User
            {
                Username = DemoAdminUsername,
                Email = DemoAdminEmail
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, DemoAdminPassword);
            context.Users.Add(admin);
        }
        else if (!HasDemoPassword(passwordHasher, admin))
        {
            admin.PasswordHash = passwordHasher.HashPassword(admin, DemoAdminPassword);
        }

        if (!admin.Roles.Any(role => role.RoleName == RoleNames.Admin))
        {
            admin.Roles.Add(adminRole);
        }

        await context.SaveChangesAsync();
    }

    // Kept for compatibility with older startup code.
    public static Task SeedRolesAsync(IServiceProvider serviceProvider) =>
        SeedAsync(serviceProvider);

    private static bool HasDemoPassword(PasswordHasher<User> passwordHasher, User admin)
    {
        try
        {
            return passwordHasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                DemoAdminPassword) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
