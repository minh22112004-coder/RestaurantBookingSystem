using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Features.Notification.Services;
using RestaurantBookingSystem.Models;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Data.Seed;

using RestaurantBookingSystem.Features.Authentication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- DbContext: dùng đúng context do leader scaffold sẵn ----
// Lưu ý: RestaurantReservationDbContext.OnConfiguring có UseSqlServer hardcode,
// nó sẽ ĐÈ chuỗi kết nối dưới đây. Chuỗi bên dưới gần như không có tác dụng
// trừ khi OnConfiguring được sửa lại (nhưng bạn xác nhận không được sửa Models).
builder.Services.AddDbContext<RestaurantReservationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ---- Services ----
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ---- JWT Authentication ----
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Không tìm thấy cấu hình Jwt:Key.");
string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Không tìm thấy cấu hình Jwt:Issuer.");
string jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Không tìm thấy cấu hình Jwt:Audience.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

// ---- Authorization policies ----
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(RoleNames.Admin)
    );

    options.AddPolicy(
        AuthorizationPolicies.ManagerOrAdmin,
        policy => policy.RequireRole(RoleNames.Manager, RoleNames.Admin)
    );

    options.AddPolicy(
        AuthorizationPolicies.AuthenticatedUser,
        policy => policy.RequireAuthenticatedUser()
    );
});

var app = builder.Build();

// Gọi seeder role (Admin, Manager, Customer)
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