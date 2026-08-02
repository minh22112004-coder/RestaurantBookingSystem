using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using RestaurantBookingSystem.Models;

using RestaurantBookingSystem.Features.Authentication.Services;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Data.Seed;

using RestaurantBookingSystem.Features.Notification.Repositories;
using RestaurantBookingSystem.Features.Notification.Services;
using RestaurantBookingSystem.Features.Reservation.Services;

using RestaurantBookingSystem.Services;
using RestaurantBookingSystem.Services.Interfaces;

using RestaurantBookingSystem.Features.Dashboard.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<RestaurantReservationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Services
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IDiningTableService, DiningTableService>();
builder.Services.AddScoped<IReportService, ReportService>();

// JWT Authentication
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Không tìm thấy cấu hình Jwt:Key."
    );

string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Không tìm thấy cấu hình Jwt:Issuer."
    );

string jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Không tìm thấy cấu hình Jwt:Audience."
    );

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(RoleNames.Admin)
    );

    options.AddPolicy(
        AuthorizationPolicies.ManagerOrAdmin,
        policy => policy.RequireRole(
            RoleNames.Manager,
            RoleNames.Admin
        )
    );

    options.AddPolicy(
        AuthorizationPolicies.AuthenticatedUser,
        policy => policy.RequireAuthenticatedUser()
    );
});

var app = builder.Build();

// Seed roles: Admin, Manager, Customer
using (IServiceScope scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello ASP.NET Core");

app.MapControllers();

app.Run();