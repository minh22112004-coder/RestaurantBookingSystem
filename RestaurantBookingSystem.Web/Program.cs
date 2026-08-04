using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Configuration;
using RestaurantBookingSystem.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BackendApiOptions>(
    builder.Configuration.GetSection(BackendApiOptions.SectionName));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RestaurantBookingSystem.Web.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromMinutes(120);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtSessionService, JwtSessionService>();
builder.Services.AddTransient<ApiAuthenticationHandler>();
builder.Services.AddScoped<ApiExceptionFilter>();

builder.Services.AddControllersWithViews(options =>
    options.Filters.AddService<ApiExceptionFilter>());

AddApiClient<IAuthApiClient, AuthApiClient>(builder.Services, builder.Configuration);
AddApiClient<IRestaurantApiClient, RestaurantApiClient>(builder.Services, builder.Configuration);
AddApiClient<IDiningTableApiClient, DiningTableApiClient>(builder.Services, builder.Configuration);
AddApiClient<IReservationApiClient, ReservationApiClient>(builder.Services, builder.Configuration);
AddApiClient<IMenuApiClient, MenuApiClient>(builder.Services, builder.Configuration);
AddApiClient<INotificationApiClient, NotificationApiClient>(builder.Services, builder.Configuration);
AddApiClient<IReportApiClient, ReportApiClient>(builder.Services, builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/error/status", "?code={0}");
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void AddApiClient<TClient, TImplementation>(
    IServiceCollection services,
    IConfiguration configuration)
    where TClient : class
    where TImplementation : class, TClient
{
    var baseUrl = configuration[$"{BackendApiOptions.SectionName}:BaseUrl"]
        ?? throw new InvalidOperationException("Missing BackendApi:BaseUrl configuration.");

    services.AddHttpClient<TClient, TImplementation>(client =>
    {
        client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }).AddHttpMessageHandler<ApiAuthenticationHandler>();
}

public partial class Program { }
