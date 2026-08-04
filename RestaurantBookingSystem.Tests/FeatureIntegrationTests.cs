using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantBookingSystem.DTOs.DiningTable;
using RestaurantBookingSystem.DTOs.Menu;
using RestaurantBookingSystem.DTOs.Restaurant;
using RestaurantBookingSystem.Features.Authentication.DTOs;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Dashboard.Dtos;
using RestaurantBookingSystem.Features.Notification.DTOs;
using RestaurantBookingSystem.Features.Reservation.DTOs;
using RestaurantBookingSystem.Models;
using Xunit;

namespace RestaurantBookingSystem.Tests;

public class FeatureIntegrationTests
{
    [Fact]
    public async Task DemoAdmin_IsSeededAndCanLoginWithUsername()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin",
            password = "123456"
        });

        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Equal("admin", auth.User.Username);
        Assert.Equal(RoleNames.Admin, auth.User.Role);
    }

    [Fact]
    public async Task Authentication_RegisterLoginAndMe_WorkEndToEnd()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var email = $"customer-{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "new-customer",
            email,
            phone = "0901234567",
            password = "Password123!"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Password123!"
        });
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Equal(RoleNames.Customer, auth.User.Role);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
            var customerRole = await context.Roles.SingleAsync(r => r.RoleName == RoleNames.Customer);
            var legacyUser = new User
            {
                Username = "legacy-user",
                Email = "legacy@example.com",
                PasswordHash = "hashed_pw_1"
            };
            legacyUser.Roles.Add(customerRole);
            context.Users.Add(legacyUser);
            await context.SaveChangesAsync();
        }
        var legacyLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "legacy@example.com",
            password = "anything"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, legacyLogin.StatusCode);
    }

    [Fact]
    public async Task RestaurantAndTable_WritesRequireAdmin_AndPersistInEfStore()
    {
        using var factory = new TestApiFactory();
        using var anonymous = factory.CreateClient();
        var request = new RestaurantCreateDto
        {
            Name = "Test Restaurant",
            Address = "1 Test Street",
            Phone = "0901234567",
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(22, 0)
        };
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/Restaurant", request)).StatusCode);

        var (_, token) = await factory.CreateUserAsync(RoleNames.Admin, "restaurant-admin");
        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createRestaurant = await admin.PostAsJsonAsync("/api/Restaurant", request);
        Assert.Equal(HttpStatusCode.Created, createRestaurant.StatusCode);
        var restaurant = await createRestaurant.Content.ReadFromJsonAsync<RestaurantResponseDto>();
        Assert.NotNull(restaurant);

        var createTable = await admin.PostAsJsonAsync("/api/DiningTable", new DiningTableCreateDto
        {
            RestaurantId = restaurant.RestaurantId,
            TableNumber = "A01",
            Capacity = 4,
            Status = "Available"
        });
        Assert.Equal(HttpStatusCode.Created, createTable.StatusCode);
        var table = await createTable.Content.ReadFromJsonAsync<DiningTableResponseDto>();
        Assert.NotNull(table);

        var publicRead = await anonymous.GetFromJsonAsync<DiningTableResponseDto>($"/api/DiningTable/{table.TableId}");
        Assert.NotNull(publicRead);
        Assert.Equal(restaurant.RestaurantId, publicRead.RestaurantId);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        Assert.True(await context.DiningTables.AnyAsync(t => t.TableId == table.TableId));
    }

    [Fact]
    public async Task Reservation_UsesJwtOwner_PreventsOverlap_AndCreatesNotification()
    {
        using var factory = new TestApiFactory();
        using var anonymous = factory.CreateClient();
        var (owner, ownerToken) = await factory.CreateUserAsync(RoleNames.Customer, "reservation-owner");
        var (other, otherToken) = await factory.CreateUserAsync(RoleNames.Customer, "reservation-other");
        var tableId = await SeedRestaurantAndTableAsync(factory);
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var request = new CreateReservationDto
        {
            TableId = tableId,
            Date = date,
            StartTime = new TimeOnly(18, 0),
            EndTime = new TimeOnly(20, 0),
            GuestCount = 4
        };

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/Reservation", request)).StatusCode);

        using var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var create = await ownerClient.PostAsJsonAsync("/api/Reservation", request);
        Assert.True(create.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but got {(int)create.StatusCode}: {await create.Content.ReadAsStringAsync()}");
        var reservation = await create.Content.ReadFromJsonAsync<ReservationResponseDto>();
        Assert.NotNull(reservation);
        Assert.Equal(owner.UserId, reservation.UserId);

        using var otherClient = factory.CreateClient();
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await otherClient.PostAsJsonAsync("/api/Reservation", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await otherClient.GetAsync($"/api/Reservation/customer/{owner.UserId}")).StatusCode);

        var update = await ownerClient.PutAsJsonAsync($"/api/Reservation/{reservation.ReservationId}",
            new UpdateReservationDto
            {
                TableId = tableId,
                Date = date,
                StartTime = new TimeOnly(19, 0),
                EndTime = new TimeOnly(21, 0),
                GuestCount = 3
            });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await ownerClient.PutAsync($"/api/Reservation/{reservation.ReservationId}/cancel", null)).StatusCode);

        var notifications = await ownerClient.GetFromJsonAsync<List<NotificationResponse>>(
            $"/api/notifications/user/{owner.UserId}");
        Assert.NotNull(notifications);
        Assert.Equal(3, notifications.Count);
        Assert.Contains(notifications, n => n.Title.Contains("request", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notifications, n => n.Title.Contains("updated", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(notifications, n => n.Title.Contains("cancelled", StringComparison.OrdinalIgnoreCase));

        Assert.NotEqual(owner.UserId, other.UserId);
    }

    [Fact]
    public async Task Reservation_RejectsInvalidTimeAndGuestCount()
    {
        using var factory = new TestApiFactory();
        var (_, token) = await factory.CreateUserAsync(RoleNames.Customer, "validation-customer");
        var tableId = await SeedRestaurantAndTableAsync(factory);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Reservation", new CreateReservationDto
        {
            TableId = tableId,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = new TimeOnly(20, 0),
            EndTime = new TimeOnly(19, 0),
            GuestCount = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Menu_ReadIsPublic_ButMutationsRequireAdmin_AndResponseHasNoReferenceCycle()
    {
        using var factory = new TestApiFactory();
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.PostAsJsonAsync("/api/Category", new CategoryRequestDto { Name = "Main" })).StatusCode);

        var (_, token) = await factory.CreateUserAsync(RoleNames.Admin, "menu-admin");
        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var restaurantId = await SeedRestaurantAsync(factory, "Menu Restaurant");
        var categoryResponse = await admin.PostAsJsonAsync("/api/Category", new CategoryRequestDto { Name = "Main" });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();
        Assert.NotNull(category);

        var menuResponse = await admin.PostAsJsonAsync("/api/MenuItem", new MenuItemRequestDto
        {
            RestaurantId = restaurantId,
            CategoryId = category.CategoryId,
            Name = "Steak",
            Price = 150000,
            Available = true
        });
        Assert.Equal(HttpStatusCode.Created, menuResponse.StatusCode);
        var menuItem = await menuResponse.Content.ReadFromJsonAsync<MenuItemResponseDto>();
        Assert.NotNull(menuItem);

        var publicResponse = await anonymous.GetAsync($"/api/MenuItem/{menuItem.MenuItemId}");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var json = await publicResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("orderItems", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("categoryName", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Notification_CanOnlyBeReadOrChangedByOwnerOrAdmin()
    {
        using var factory = new TestApiFactory();
        var (owner, ownerToken) = await factory.CreateUserAsync(RoleNames.Customer, "notification-owner");
        var (_, otherToken) = await factory.CreateUserAsync(RoleNames.Customer, "notification-other");
        int notificationId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
            var notification = new Notification
            {
                UserId = owner.UserId,
                Title = "Test",
                Message = "Private",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
            notificationId = notification.NotificationId;
        }

        using var other = factory.CreateClient();
        other.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await other.GetAsync($"/api/notifications/user/{owner.UserId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await other.PutAsync($"/api/notifications/{notificationId}/read", null)).StatusCode);

        using var ownerClient = factory.CreateClient();
        ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        Assert.Equal(HttpStatusCode.NoContent,
            (await ownerClient.PutAsync($"/api/notifications/{notificationId}/read", null)).StatusCode);
    }

    [Fact]
    public async Task Dashboard_IsAdminOnly_AndRestaurantFilterScopesOverview()
    {
        using var factory = new TestApiFactory();
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/dashboard/overview")).StatusCode);

        var (_, adminToken) = await factory.CreateUserAsync(RoleNames.Admin, "dashboard-admin");
        var (customer1, _) = await factory.CreateUserAsync(RoleNames.Customer, "dashboard-customer-1");
        var (customer2, _) = await factory.CreateUserAsync(RoleNames.Customer, "dashboard-customer-2");
        var restaurant1 = await SeedRestaurantAsync(factory, "Dashboard One");
        var restaurant2 = await SeedRestaurantAsync(factory, "Dashboard Two");
        await SeedPaidReservationAsync(factory, customer1.UserId, restaurant1, 100m);
        await SeedPaidReservationAsync(factory, customer2.UserId, restaurant2, 900m);

        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var overview = await admin.GetFromJsonAsync<DashboardOverviewDto>(
            $"/api/dashboard/overview?restaurantId={restaurant1}");
        Assert.NotNull(overview);
        Assert.Equal(100m, overview.TodayRevenue);
        Assert.Equal(1, overview.TodayReservations);
        Assert.Equal(1, overview.TotalCustomers);

        var invalidFilter = await admin.GetAsync("/api/reports/revenue?from=2026-08-03&to=2026-08-01");
        Assert.Equal(HttpStatusCode.BadRequest, invalidFilter.StatusCode);
    }

    private static async Task<int> SeedRestaurantAsync(TestApiFactory factory, string name)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        var restaurant = new Restaurant
        {
            Name = name,
            Address = "Test",
            Phone = "0900000000",
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(22, 0)
        };
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();
        return restaurant.RestaurantId;
    }

    private static async Task<int> SeedRestaurantAndTableAsync(TestApiFactory factory)
    {
        var restaurantId = await SeedRestaurantAsync(factory, $"Reservation Restaurant {Guid.NewGuid():N}");
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        var table = new DiningTable
        {
            RestaurantId = restaurantId,
            TableNumber = $"T-{Guid.NewGuid():N}"[..20],
            Capacity = 4,
            Status = "Available"
        };
        context.DiningTables.Add(table);
        await context.SaveChangesAsync();
        return table.TableId;
    }

    private static async Task SeedPaidReservationAsync(
        TestApiFactory factory,
        int userId,
        int restaurantId,
        decimal total)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        var table = new DiningTable
        {
            RestaurantId = restaurantId,
            TableNumber = $"D-{Guid.NewGuid():N}"[..20],
            Capacity = 4,
            Status = "Occupied"
        };
        var reservation = new Reservation
        {
            UserId = userId,
            Table = table,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(18, 0),
            EndTime = new TimeOnly(20, 0),
            GuestCount = 2,
            Status = "Confirmed"
        };
        var order = new Order
        {
            Reservation = reservation,
            TotalAmount = total,
            PaymentStatus = "Paid",
            CreatedAt = DateTime.Now
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
    }
}
