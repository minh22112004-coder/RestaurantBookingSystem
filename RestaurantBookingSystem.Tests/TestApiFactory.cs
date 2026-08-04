using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RestaurantBookingSystem.Features.Authentication.Services;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"RestaurantBookingTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<RestaurantReservationDbContext>();
            services.RemoveAll<DbContextOptions<RestaurantReservationDbContext>>();
            services.RemoveAll<IDatabaseProvider>();
            var inMemoryServices = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            services.AddDbContext<RestaurantReservationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .UseInternalServiceProvider(inMemoryServices));
        });
    }

    public async Task<(User User, string Token)> CreateUserAsync(string roleName, string? username = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        var role = await context.Roles.SingleAsync(r => r.RoleName == roleName);
        var uniqueName = username ?? $"user-{Guid.NewGuid():N}";
        var user = new User
        {
            Username = uniqueName,
            Email = $"{uniqueName}@example.com",
            Phone = "0900000000"
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Password123!");
        user.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return (user, tokenService.CreateToken(user, new[] { roleName }));
    }
}
