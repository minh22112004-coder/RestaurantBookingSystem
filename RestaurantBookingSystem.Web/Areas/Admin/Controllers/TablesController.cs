using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/tables")]
[RequireSessionRole("Admin")]
public sealed class TablesController : Controller
{
    private readonly IDiningTableApiClient _tables;
    private readonly IRestaurantApiClient _restaurants;

    public TablesController(IDiningTableApiClient tables, IRestaurantApiClient restaurants)
    {
        _tables = tables;
        _restaurants = restaurants;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? restaurantId, CancellationToken cancellationToken)
    {
        var restaurants = await _restaurants.GetAllAsync(cancellationToken);
        var tables = restaurantId.HasValue
            ? await _tables.GetByRestaurantAsync(restaurantId.Value, cancellationToken)
            : await _tables.GetAllAsync(cancellationToken);
        return View(new AdminTableIndexViewModel
        {
            Restaurants = restaurants, Tables = tables, SelectedRestaurantId = restaurantId,
            Form = new AdminTableFormViewModel { RestaurantId = restaurantId ?? restaurants.FirstOrDefault()?.RestaurantId ?? 0 }
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] AdminTableFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Index", await LoadIndexAsync(form.RestaurantId, form, cancellationToken));

        try
        {
            await _tables.CreateAsync(ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Table created successfully.";
            return RedirectToAction(nameof(Index), new { restaurantId = form.RestaurantId });
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The table could not be created. Its number may already be in use.");
            return View("Index", await LoadIndexAsync(form.RestaurantId, form, cancellationToken));
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var table = await _tables.GetByIdAsync(id, cancellationToken);
            return View(new AdminTableEditViewModel
            {
                TableId = id, Restaurants = await _restaurants.GetAllAsync(cancellationToken), Form = FromDto(table)
            });
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Form")] AdminTableFormViewModel form, CancellationToken cancellationToken)
    {
        var model = new AdminTableEditViewModel { TableId = id, Restaurants = await _restaurants.GetAllAsync(cancellationToken), Form = form };
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _tables.UpdateAsync(id, ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Table updated successfully.";
            return RedirectToAction(nameof(Index), new { restaurantId = form.RestaurantId });
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The table could not be updated. Check its number and status.");
            return View(model);
        }
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            await _tables.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Table deleted successfully.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["ErrorMessage"] = "This table cannot be deleted because it has reservation history.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "The table no longer exists.";
        }
        return RedirectToAction(nameof(Index), new { restaurantId });
    }

    private async Task<AdminTableIndexViewModel> LoadIndexAsync(int? restaurantId, AdminTableFormViewModel form, CancellationToken token) => new()
    {
        Restaurants = await _restaurants.GetAllAsync(token),
        Tables = restaurantId.HasValue ? await _tables.GetByRestaurantAsync(restaurantId.Value, token) : await _tables.GetAllAsync(token),
        SelectedRestaurantId = restaurantId,
        Form = form
    };

    private static bool IsExpected(ApiClientException exception) =>
        exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict;

    private static DiningTableWriteRequest ToRequest(AdminTableFormViewModel form) => new()
    {
        RestaurantId = form.RestaurantId, TableNumber = form.TableNumber.Trim(), Capacity = form.Capacity, Status = form.Status
    };

    private static AdminTableFormViewModel FromDto(DiningTableDto table) => new()
    {
        RestaurantId = table.RestaurantId, TableNumber = table.TableNumber, Capacity = table.Capacity, Status = table.Status
    };
}
