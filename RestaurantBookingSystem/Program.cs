using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Features.Notification.Repositories;
using RestaurantBookingSystem.Features.Notification.Services;
using RestaurantBookingSystem.Features.Reservation.Services;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Services;
using RestaurantBookingSystem.Services.Interfaces;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RestaurantReservationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IDiningTableService, DiningTableService>();

// ...
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapGet("/", () => "Hello ASP.NET Core");

app.MapControllers();

app.Run();
