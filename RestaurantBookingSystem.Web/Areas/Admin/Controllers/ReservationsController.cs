using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/reservations")]
[RequireSessionRole("Admin")]
public sealed class ReservationsController : Controller
{
    private readonly IReservationApiClient _reservations;
    private readonly IDiningTableApiClient _tables;
    private readonly IRestaurantApiClient _restaurants;

    public ReservationsController(
        IReservationApiClient reservations,
        IDiningTableApiClient tables,
        IRestaurantApiClient restaurants)
    {
        _reservations = reservations;
        _tables = tables;
        _restaurants = restaurants;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? date,
        int? restaurantId,
        string? status,
        CancellationToken cancellationToken)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var reservationsTask = _reservations.GetByDateAsync(selectedDate, cancellationToken);
        var tablesTask = _tables.GetAllAsync(cancellationToken);
        var restaurantsTask = _restaurants.GetAllAsync(cancellationToken);
        await Task.WhenAll(reservationsTask, tablesTask, restaurantsTask);

        var tableMap = (await tablesTask).ToDictionary(item => item.TableId);
        var restaurantMap = (await restaurantsTask).ToDictionary(item => item.RestaurantId);
        var rows = (await reservationsTask).Select(reservation =>
        {
            tableMap.TryGetValue(reservation.TableId, out var table);
            var restaurant = table is null ? null : restaurantMap.GetValueOrDefault(table.RestaurantId);
            return new AdminReservationRowViewModel
            {
                Reservation = reservation,
                TableNumber = table?.TableNumber ?? "Unknown table",
                RestaurantName = restaurant?.Name ?? "Unknown restaurant"
            };
        });

        if (restaurantId.HasValue)
            rows = rows.Where(row => tableMap.GetValueOrDefault(row.Reservation.TableId)?.RestaurantId == restaurantId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            rows = rows.Where(row => string.Equals(row.Reservation.Status, status, StringComparison.OrdinalIgnoreCase));

        return View(new AdminReservationIndexViewModel
        {
            Date = selectedDate,
            RestaurantId = restaurantId,
            Status = status ?? string.Empty,
            Restaurants = await restaurantsTask,
            Reservations = rows.OrderBy(row => row.Reservation.StartTime).ToList()
        });
    }

    [HttpPost("{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        int id,
        DateOnly date,
        int? restaurantId,
        string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            await _reservations.CancelAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
        }
        catch (ApiClientException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
        {
            TempData["ErrorMessage"] = "The reservation could not be cancelled. It may already have changed.";
        }
        return RedirectToAction(nameof(Index), new { date, restaurantId, status });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, DateOnly date, CancellationToken cancellationToken)
    {
        var reservation = (await _reservations.GetByDateAsync(date, cancellationToken))
            .FirstOrDefault(item => item.ReservationId == id);
        if (reservation is null)
            return NotFound();
        if (string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Cancelled reservations cannot be updated.";
            return RedirectToAction(nameof(Index), new { date });
        }

        return View(await LoadEditAsync(reservation, new ReservationFormViewModel
        {
            TableId = reservation.TableId,
            Date = reservation.Date,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            GuestCount = reservation.GuestCount
        }, cancellationToken));
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        int userId,
        string status,
        [Bind(Prefix = "Form")] ReservationFormViewModel form,
        CancellationToken cancellationToken)
    {
        var source = new ReservationDto { ReservationId = id, UserId = userId, Status = status };
        var model = await LoadEditAsync(source, form, cancellationToken);
        ValidateTable(model);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _reservations.UpdateAsync(id, new ReservationWriteRequest
            {
                TableId = form.TableId,
                Date = form.Date,
                StartTime = form.StartTime,
                EndTime = form.EndTime,
                GuestCount = form.GuestCount
            }, cancellationToken);
            TempData["SuccessMessage"] = "Reservation updated successfully.";
            return RedirectToAction(nameof(Index), new { date = form.Date });
        }
        catch (ApiClientException exception) when (exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            ModelState.AddModelError(string.Empty, "The reservation could not be updated. The table may no longer be available.");
            return View(model);
        }
    }

    private async Task<AdminReservationEditViewModel> LoadEditAsync(
        ReservationDto reservation,
        ReservationFormViewModel form,
        CancellationToken cancellationToken)
    {
        var tablesTask = _tables.GetAllAsync(cancellationToken);
        var restaurantsTask = _restaurants.GetAllAsync(cancellationToken);
        await Task.WhenAll(tablesTask, restaurantsTask);
        return new AdminReservationEditViewModel
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            Status = reservation.Status,
            Tables = await tablesTask,
            Restaurants = await restaurantsTask,
            Form = form
        };
    }

    private void ValidateTable(AdminReservationEditViewModel model)
    {
        var table = model.Tables.FirstOrDefault(item => item.TableId == model.Form.TableId);
        if (table is null || string.Equals(table.Status, "Maintenance", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Form.TableId", "Please choose an available table.");
            return;
        }
        if (model.Form.GuestCount > table.Capacity)
            ModelState.AddModelError("Form.GuestCount", $"This table seats a maximum of {table.Capacity} guests.");

        var restaurant = model.Restaurants.FirstOrDefault(item => item.RestaurantId == table.RestaurantId);
        if (restaurant is not null && (model.Form.StartTime < restaurant.OpenTime || model.Form.EndTime > restaurant.CloseTime))
        {
            ModelState.AddModelError(
                "Form.EndTime",
                $"Reservation times must be between {restaurant.OpenTime:HH:mm} and {restaurant.CloseTime:HH:mm}.");
        }
    }
}
