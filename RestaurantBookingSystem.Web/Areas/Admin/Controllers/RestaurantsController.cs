using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/restaurants")]
[RequireSessionRole("Admin")]
public sealed class RestaurantsController : Controller
{
    private readonly IRestaurantApiClient _client;

    public RestaurantsController(IRestaurantApiClient client) => _client = client;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(new AdminRestaurantIndexViewModel { Restaurants = await _client.GetAllAsync(cancellationToken) });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] AdminRestaurantFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Index", new AdminRestaurantIndexViewModel { Restaurants = await _client.GetAllAsync(cancellationToken), Form = form });

        try
        {
            await _client.CreateAsync(ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Restaurant created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The restaurant could not be created. Check the details and try again.");
            return View("Index", new AdminRestaurantIndexViewModel { Restaurants = await _client.GetAllAsync(cancellationToken), Form = form });
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _client.GetByIdAsync(id, cancellationToken);
            return View(new AdminRestaurantEditViewModel { RestaurantId = id, Form = FromDto(item) });
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Form")] AdminRestaurantFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(new AdminRestaurantEditViewModel { RestaurantId = id, Form = form });

        try
        {
            await _client.UpdateAsync(id, ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Restaurant updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The restaurant could not be updated. Check the details and try again.");
            return View(new AdminRestaurantEditViewModel { RestaurantId = id, Form = form });
        }
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Restaurant deleted successfully.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["ErrorMessage"] = "This restaurant cannot be deleted because related data is still in use.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "The restaurant no longer exists.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static bool IsExpected(ApiClientException exception) =>
        exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict;

    private static RestaurantWriteRequest ToRequest(AdminRestaurantFormViewModel form) => new()
    {
        Name = form.Name.Trim(), Address = form.Address.Trim(), Phone = form.Phone.Trim(),
        OpenTime = form.OpenTime, CloseTime = form.CloseTime
    };

    private static AdminRestaurantFormViewModel FromDto(RestaurantDto item) => new()
    {
        Name = item.Name, Address = item.Address, Phone = item.Phone,
        OpenTime = item.OpenTime, CloseTime = item.CloseTime
    };
}
