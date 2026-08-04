using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Controllers;

[Route("reservations")]
[RequireSessionRole("Customer")]
public sealed class ReservationViewController : Controller
{
    private readonly IReservationApiClient _reservationApiClient;
    private readonly IDiningTableApiClient _tableApiClient;
    private readonly IRestaurantApiClient _restaurantApiClient;
    private readonly ILogger<ReservationViewController> _logger;

    public ReservationViewController(
        IReservationApiClient reservationApiClient,
        IDiningTableApiClient tableApiClient,
        IRestaurantApiClient restaurantApiClient,
        ILogger<ReservationViewController> logger)
    {
        _reservationApiClient = reservationApiClient;
        _tableApiClient = tableApiClient;
        _restaurantApiClient = restaurantApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var reservationsTask = _reservationApiClient.GetMineAsync(cancellationToken);
            var tablesTask = _tableApiClient.GetAllAsync(cancellationToken);
            var restaurantsTask = _restaurantApiClient.GetAllAsync(cancellationToken);
            await Task.WhenAll(reservationsTask, tablesTask, restaurantsTask);

            var tables = (await tablesTask).ToDictionary(table => table.TableId);
            var restaurants = (await restaurantsTask).ToDictionary(restaurant => restaurant.RestaurantId);
            var items = (await reservationsTask).Select(reservation =>
            {
                tables.TryGetValue(reservation.TableId, out var table);
                RestaurantDto? restaurant = null;
                if (table is not null)
                    restaurants.TryGetValue(table.RestaurantId, out restaurant);

                return new ReservationListItemViewModel
                {
                    Reservation = reservation,
                    RestaurantName = restaurant?.Name ?? "Restaurant unavailable",
                    TableNumber = table?.TableNumber ?? reservation.TableId.ToString(),
                    TableCapacity = table?.Capacity ?? 0
                };
            }).ToList();

            return View("~/Views/Reservation/Index.cshtml", new ReservationListViewModel { Items = items });
        }
        catch (ApiClientException exception)
        {
            _logger.LogWarning(exception, "Unable to load customer reservations.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("~/Views/Reservation/Index.cshtml", new ReservationListViewModel
            {
                ErrorMessage = "Your reservations are temporarily unavailable. Please try again shortly."
            });
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var reservation = await FindOwnedReservationAsync(id, cancellationToken);
        if (reservation is null)
            return NotFoundError("The requested reservation could not be found.");
        if (IsCancelled(reservation))
            return ConflictError("Cancelled reservations cannot be updated.");

        var form = new ReservationFormViewModel
        {
            TableId = reservation.TableId,
            Date = reservation.Date,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            GuestCount = reservation.GuestCount
        };
        return View("~/Views/Reservation/Edit.cshtml", await LoadEditViewModelAsync(reservation, form, cancellationToken));
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(Prefix = "Form")] ReservationFormViewModel form,
        CancellationToken cancellationToken)
    {
        var reservation = await FindOwnedReservationAsync(id, cancellationToken);
        if (reservation is null)
            return NotFoundError("The requested reservation could not be found.");
        if (IsCancelled(reservation))
            return ConflictError("Cancelled reservations cannot be updated.");

        var model = await LoadEditViewModelAsync(reservation, form, cancellationToken);
        ValidateAgainstTable(model, form);
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("~/Views/Reservation/Edit.cshtml", model);
        }

        try
        {
            await _reservationApiClient.UpdateAsync(id, ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Your reservation has been updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException exception) when (
            exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            _logger.LogWarning(exception, "Reservation {ReservationId} update was rejected.", id);
            ModelState.AddModelError(
                string.Empty,
                "The reservation could not be updated with those details. The table may no longer be available.");
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("~/Views/Reservation/Edit.cshtml", model);
        }
    }

    [HttpPost("{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var reservation = await FindOwnedReservationAsync(id, cancellationToken);
        if (reservation is null)
            return NotFoundError("The requested reservation could not be found.");

        await _reservationApiClient.CancelAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Your reservation has been cancelled.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ReservationDto?> FindOwnedReservationAsync(int id, CancellationToken cancellationToken) =>
        (await _reservationApiClient.GetMineAsync(cancellationToken))
            .FirstOrDefault(reservation => reservation.ReservationId == id);

    private async Task<ReservationEditViewModel> LoadEditViewModelAsync(
        ReservationDto reservation,
        ReservationFormViewModel form,
        CancellationToken cancellationToken)
    {
        DiningTableDto table;
        try
        {
            table = await _tableApiClient.GetByIdAsync(form.TableId, cancellationToken);
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            ModelState.AddModelError("Form.TableId", "Please choose a valid table.");
            table = await _tableApiClient.GetByIdAsync(reservation.TableId, cancellationToken);
        }
        var restaurantTask = _restaurantApiClient.GetByIdAsync(table.RestaurantId, cancellationToken);
        var tablesTask = _tableApiClient.GetByRestaurantAsync(table.RestaurantId, cancellationToken);
        await Task.WhenAll(restaurantTask, tablesTask);

        var restaurant = await restaurantTask;
        return new ReservationEditViewModel
        {
            ReservationId = reservation.ReservationId,
            RestaurantName = restaurant.Name,
            Status = reservation.Status,
            OpenTime = restaurant.OpenTime,
            CloseTime = restaurant.CloseTime,
            Form = form,
            Tables = await tablesTask
        };
    }

    private void ValidateAgainstTable(ReservationEditViewModel model, ReservationFormViewModel form)
    {
        var table = model.Tables.FirstOrDefault(item => item.TableId == form.TableId);
        if (table is null || string.Equals(table.Status, "Maintenance", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Form.TableId", "Please choose an available table.");
            return;
        }

        if (form.GuestCount > table.Capacity)
            ModelState.AddModelError("Form.GuestCount", $"This table seats a maximum of {table.Capacity} guests.");

        if (form.StartTime < model.OpenTime || form.EndTime > model.CloseTime)
        {
            ModelState.AddModelError(
                "Form.EndTime",
                $"Reservation times must be between {model.OpenTime:HH:mm} and {model.CloseTime:HH:mm}.");
        }
    }

    private ViewResult NotFoundError(string message)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return ErrorView(404, "Reservation not found", message);
    }

    private ViewResult ConflictError(string message)
    {
        Response.StatusCode = StatusCodes.Status409Conflict;
        return ErrorView(409, "Reservation cannot be updated", message);
    }

    private ViewResult ErrorView(int statusCode, string title, string message) =>
        View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            StatusCode = statusCode,
            Title = title,
            Message = message
        });

    private static bool IsCancelled(ReservationDto reservation) =>
        string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private static ReservationWriteRequest ToRequest(ReservationFormViewModel form) => new()
    {
        TableId = form.TableId,
        Date = form.Date,
        StartTime = form.StartTime,
        EndTime = form.EndTime,
        GuestCount = form.GuestCount
    };
}
