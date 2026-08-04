using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.Menu;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuItemController : ControllerBase
{
    private readonly RestaurantReservationDbContext _context;

    public MenuItemController(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItemResponseDto>>> GetMenuItems([FromQuery] int? restaurantId)
    {
        var query = _context.MenuItems.AsNoTracking().AsQueryable();
        if (restaurantId.HasValue)
            query = query.Where(m => m.RestaurantId == restaurantId.Value);
        return await Project(query).OrderBy(m => m.MenuItemId).ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuItemResponseDto>> GetMenuItem(int id)
    {
        var item = await Project(_context.MenuItems.AsNoTracking().Where(m => m.MenuItemId == id))
            .FirstOrDefaultAsync();
        return item is null ? NotFound() : item;
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<MenuItemResponseDto>> CreateMenuItem(MenuItemRequestDto request)
    {
        var validationResult = await ValidateReferencesAsync(request);
        if (validationResult is not null)
            return validationResult;

        var menuItem = new MenuItem
        {
            RestaurantId = request.RestaurantId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Price = request.Price,
            Available = request.Available
        };
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();
        var response = await Project(_context.MenuItems.AsNoTracking().Where(m => m.MenuItemId == menuItem.MenuItemId))
            .SingleAsync();
        return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.MenuItemId }, response);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMenuItem(int id, MenuItemRequestDto request)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem is null)
            return NotFound();
        var validationResult = await ValidateReferencesAsync(request);
        if (validationResult is not null)
            return validationResult;

        menuItem.RestaurantId = request.RestaurantId;
        menuItem.CategoryId = request.CategoryId;
        menuItem.Name = request.Name.Trim();
        menuItem.Price = request.Price;
        menuItem.Available = request.Available;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem is null)
            return NotFound();
        if (await _context.OrderItems.AnyAsync(oi => oi.MenuItemId == id))
            return Conflict(new { message = "The menu item appears in an order and cannot be deleted." });

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult?> ValidateReferencesAsync(MenuItemRequestDto request)
    {
        if (!await _context.Restaurants.AnyAsync(r => r.RestaurantId == request.RestaurantId))
            return NotFound(new { message = "Restaurant not found." });
        if (!await _context.Categories.AnyAsync(c => c.CategoryId == request.CategoryId))
            return NotFound(new { message = "Category not found." });
        return null;
    }

    private static IQueryable<MenuItemResponseDto> Project(IQueryable<MenuItem> query) =>
        query.Select(m => new MenuItemResponseDto
        {
            MenuItemId = m.MenuItemId,
            RestaurantId = m.RestaurantId ?? 0,
            RestaurantName = m.Restaurant != null ? m.Restaurant.Name : string.Empty,
            CategoryId = m.CategoryId ?? 0,
            CategoryName = m.Category != null ? m.Category.Name : string.Empty,
            Name = m.Name,
            Price = m.Price,
            Available = m.Available ?? false
        });
}
