using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
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
    }
}