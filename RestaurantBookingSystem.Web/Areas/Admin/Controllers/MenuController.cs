using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/menu")]
[RequireSessionRole("Admin")]
public sealed class MenuController : Controller
{
    private readonly IMenuApiClient _menu;
    private readonly IRestaurantApiClient _restaurants;

    public MenuController(IMenuApiClient menu, IRestaurantApiClient restaurants)
    {
        _menu = menu;
        _restaurants = restaurants;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? restaurantId, CancellationToken cancellationToken) =>
        View(await LoadIndexAsync(restaurantId, null, null, cancellationToken));

    [HttpPost("categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(
        [Bind(Prefix = "CategoryForm")] AdminCategoryFormViewModel form,
        int? restaurantId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Index", await LoadIndexAsync(restaurantId, form, null, cancellationToken));

        try
        {
            await _menu.CreateCategoryAsync(new CategoryWriteRequest { Name = form.Name.Trim() }, cancellationToken);
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index), new { restaurantId });
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The category could not be created. Its name may already exist.");
            return View("Index", await LoadIndexAsync(restaurantId, form, null, cancellationToken));
        }
    }

    [HttpGet("categories/{id:int}/edit")]
    public async Task<IActionResult> EditCategory(int id, CancellationToken cancellationToken)
    {
        var category = (await _menu.GetCategoriesAsync(cancellationToken)).FirstOrDefault(item => item.CategoryId == id);
        return category is null
            ? NotFound()
            : View(new AdminCategoryEditViewModel
            {
                CategoryId = id, Form = new AdminCategoryFormViewModel { Name = category.Name }
            });
    }

    [HttpPost("categories/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, [Bind(Prefix = "Form")] AdminCategoryFormViewModel form, CancellationToken cancellationToken)
    {
        var model = new AdminCategoryEditViewModel { CategoryId = id, Form = form };
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _menu.UpdateCategoryAsync(id, new CategoryWriteRequest { Name = form.Name.Trim() }, cancellationToken);
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The category could not be updated. Its name may already exist.");
            return View(model);
        }
    }

    [HttpPost("categories/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int? restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            await _menu.DeleteCategoryAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Category deleted successfully.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["ErrorMessage"] = "This category cannot be deleted because it contains menu items.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "The category no longer exists.";
        }
        return RedirectToAction(nameof(Index), new { restaurantId });
    }

    [HttpPost("items/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(
        [Bind(Prefix = "ItemForm")] AdminMenuItemFormViewModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Index", await LoadIndexAsync(form.RestaurantId, null, form, cancellationToken));

        try
        {
            await _menu.CreateItemAsync(ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Menu item created successfully.";
            return RedirectToAction(nameof(Index), new { restaurantId = form.RestaurantId });
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The menu item could not be created. Check the restaurant and category.");
            return View("Index", await LoadIndexAsync(form.RestaurantId, null, form, cancellationToken));
        }
    }

    [HttpGet("items/{id:int}/edit")]
    public async Task<IActionResult> EditItem(int id, CancellationToken cancellationToken)
    {
        var items = await _menu.GetItemsAsync(null, cancellationToken);
        var item = items.FirstOrDefault(value => value.MenuItemId == id);
        if (item is null)
            return NotFound();
        return View(new AdminMenuItemEditViewModel
        {
            MenuItemId = id,
            Restaurants = await _restaurants.GetAllAsync(cancellationToken),
            Categories = await _menu.GetCategoriesAsync(cancellationToken),
            Form = FromDto(item)
        });
    }

    [HttpPost("items/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(int id, [Bind(Prefix = "Form")] AdminMenuItemFormViewModel form, CancellationToken cancellationToken)
    {
        var model = new AdminMenuItemEditViewModel
        {
            MenuItemId = id,
            Restaurants = await _restaurants.GetAllAsync(cancellationToken),
            Categories = await _menu.GetCategoriesAsync(cancellationToken),
            Form = form
        };
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _menu.UpdateItemAsync(id, ToRequest(form), cancellationToken);
            TempData["SuccessMessage"] = "Menu item updated successfully.";
            return RedirectToAction(nameof(Index), new { restaurantId = form.RestaurantId });
        }
        catch (ApiClientException exception) when (IsExpected(exception))
        {
            ModelState.AddModelError(string.Empty, "The menu item could not be updated. Check the restaurant and category.");
            return View(model);
        }
    }

    [HttpPost("items/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id, int? restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            await _menu.DeleteItemAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Menu item deleted successfully.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["ErrorMessage"] = "This menu item cannot be deleted because it appears in an order.";
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "The menu item no longer exists.";
        }
        return RedirectToAction(nameof(Index), new { restaurantId });
    }

    private async Task<AdminMenuIndexViewModel> LoadIndexAsync(
        int? restaurantId,
        AdminCategoryFormViewModel? categoryForm,
        AdminMenuItemFormViewModel? itemForm,
        CancellationToken cancellationToken)
    {
        var restaurantsTask = _restaurants.GetAllAsync(cancellationToken);
        var categoriesTask = _menu.GetCategoriesAsync(cancellationToken);
        var itemsTask = _menu.GetItemsAsync(restaurantId, cancellationToken);
        await Task.WhenAll(restaurantsTask, categoriesTask, itemsTask);
        var restaurants = await restaurantsTask;
        var categories = await categoriesTask;
        return new AdminMenuIndexViewModel
        {
            Restaurants = restaurants,
            Categories = categories,
            Items = await itemsTask,
            SelectedRestaurantId = restaurantId,
            CategoryForm = categoryForm ?? new(),
            ItemForm = itemForm ?? new AdminMenuItemFormViewModel
            {
                RestaurantId = restaurantId ?? restaurants.FirstOrDefault()?.RestaurantId ?? 0,
                CategoryId = categories.FirstOrDefault()?.CategoryId ?? 0
            }
        };
    }

    private static bool IsExpected(ApiClientException exception) =>
        exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict;

    private static MenuItemWriteRequest ToRequest(AdminMenuItemFormViewModel form) => new()
    {
        RestaurantId = form.RestaurantId, CategoryId = form.CategoryId, Name = form.Name.Trim(),
        Price = form.Price, Available = form.Available
    };

    private static AdminMenuItemFormViewModel FromDto(MenuItemDto item) => new()
    {
        RestaurantId = item.RestaurantId, CategoryId = item.CategoryId, Name = item.Name,
        Price = item.Price, Available = item.Available
    };
}
