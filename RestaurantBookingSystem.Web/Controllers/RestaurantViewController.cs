using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Controllers;

[Route("restaurants")]
public sealed class RestaurantViewController : Controller
{
    private readonly IRestaurantApiClient _restaurantApiClient;
    private readonly IMenuApiClient _menuApiClient;
    private readonly IDiningTableApiClient _tableApiClient;
    private readonly IReservationApiClient _reservationApiClient;
    private readonly ILogger<RestaurantViewController> _logger;

    public RestaurantViewController(
        IRestaurantApiClient restaurantApiClient,
        IMenuApiClient menuApiClient,
        IDiningTableApiClient tableApiClient,
        IReservationApiClient reservationApiClient,
        ILogger<RestaurantViewController> logger)
    {
        _restaurantApiClient = restaurantApiClient;
        _menuApiClient = menuApiClient;
        _tableApiClient = tableApiClient;
        _reservationApiClient = reservationApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var normalizedSearch = search?.Trim() ?? string.Empty;
        try
        {
            var restaurants = await _restaurantApiClient.GetAllAsync(cancellationToken);
            if (normalizedSearch.Length > 0)
            {
                restaurants = restaurants
                    .Where(restaurant =>
                        restaurant.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                        restaurant.Address.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(new RestaurantListViewModel
            {
                Restaurants = restaurants,
                SearchTerm = normalizedSearch
            });
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load the public restaurant directory.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(new RestaurantListViewModel
            {
                SearchTerm = normalizedSearch,
                ErrorMessage = "The restaurant directory is temporarily unavailable. Please try again shortly."
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await LoadDetailsAsync(id, null, cancellationToken);
        return View(model);
    }

    [HttpPost("{id:int}/reservations")]
    [ValidateAntiForgeryToken]
    [RequireSessionRole("Customer")]
    public async Task<IActionResult> Book(
        int id,
        [Bind(Prefix = "BookingForm")] ReservationFormViewModel form,
        CancellationToken cancellationToken)
    {
        var model = await LoadDetailsAsync(id, form, cancellationToken);
        if (model.Restaurant is null || model.HasError)
            return View("Details", model);

        ValidateReservationAgainstRestaurant(model.Restaurant, model.Tables, form);
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("Details", model);
        }

        try
        {
            await _reservationApiClient.CreateAsync(ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Your reservation request has been submitted.";
            return RedirectToAction("Index", "ReservationView");
        }
        catch (ApiClientException exception) when (
            exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            _logger.LogWarning(exception, "Reservation creation was rejected for restaurant {RestaurantId}.", id);
            ModelState.AddModelError(
                string.Empty,
                "This table is not available for the selected date, time, or party size. Please choose another option.");
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("Details", model);
        }
    }

    private async Task<RestaurantDetailsViewModel> LoadDetailsAsync(
        int id,
        ReservationFormViewModel? bookingForm,
        CancellationToken cancellationToken)
    {
        RestaurantDto restaurant;
        try
        {
            restaurant = await _restaurantApiClient.GetByIdAsync(id, cancellationToken);
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new RestaurantDetailsViewModel
            {
                BookingForm = bookingForm ?? new(),
                ErrorMessage = "We could not find that restaurant. It may have been removed."
            };
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load restaurant {RestaurantId}.", id);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return new RestaurantDetailsViewModel
            {
                BookingForm = bookingForm ?? new(),
                ErrorMessage = "Restaurant details are temporarily unavailable. Please try again shortly."
            };
        }

        try
        {
            var menuTask = _menuApiClient.GetItemsAsync(id, cancellationToken);
            var tablesTask = _tableApiClient.GetByRestaurantAsync(id, cancellationToken);
            await Task.WhenAll(menuTask, tablesTask);
            var tables = await tablesTask;

            return new RestaurantDetailsViewModel
            {
                Restaurant = restaurant,
                MenuItems = await menuTask,
                Tables = tables,
                BookingForm = bookingForm ?? CreateDefaultBookingForm(restaurant, tables)
            };
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load public data for restaurant {RestaurantId}.", id);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return new RestaurantDetailsViewModel
            {
                Restaurant = restaurant,
                BookingForm = bookingForm ?? new(),
                ErrorMessage = "The menu and table information could not be loaded. Please try again shortly."
            };
        }
    }

    private void ValidateReservationAgainstRestaurant(
        RestaurantDto restaurant,
        IReadOnlyList<DiningTableDto> tables,
        ReservationFormViewModel form)
    {
        var table = tables.FirstOrDefault(item => item.TableId == form.TableId);
        if (table is null || !string.Equals(table.Status, "Available", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("BookingForm.TableId", "Please choose an available table at this restaurant.");
            return;
        }

        if (form.GuestCount > table.Capacity)
            ModelState.AddModelError("BookingForm.GuestCount", $"This table seats a maximum of {table.Capacity} guests.");

        if (form.StartTime < restaurant.OpenTime || form.EndTime > restaurant.CloseTime)
        {
            ModelState.AddModelError(
                "BookingForm.EndTime",
                $"Reservation times must be between {restaurant.OpenTime:HH:mm} and {restaurant.CloseTime:HH:mm}.");
        }
    }

    private static ReservationFormViewModel CreateDefaultBookingForm(
        RestaurantDto restaurant,
        IReadOnlyList<DiningTableDto> tables)
    {
        var endTime = restaurant.OpenTime.AddHours(2);
        if (endTime > restaurant.CloseTime)
            endTime = restaurant.CloseTime;

        return new ReservationFormViewModel
        {
            TableId = tables.FirstOrDefault(table =>
                string.Equals(table.Status, "Available", StringComparison.OrdinalIgnoreCase))?.TableId ?? 0,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            StartTime = restaurant.OpenTime,
            EndTime = endTime,
            GuestCount = 2
        };
    }

    private static ReservationWriteRequest ToRequest(ReservationFormViewModel form) => new()
    {
        TableId = form.TableId,
        Date = form.Date,
        StartTime = form.StartTime,
        EndTime = form.EndTime,
        GuestCount = form.GuestCount
    };
}
