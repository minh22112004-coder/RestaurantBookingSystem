using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.Menu;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly RestaurantReservationDbContext _context;

    public CategoryController(RestaurantReservationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        return await _context.Categories.AsNoTracking()
            .OrderBy(c => c.CategoryId)
            .Select(c => new CategoryResponseDto { CategoryId = c.CategoryId, Name = c.Name })
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
    {
        var category = await _context.Categories.AsNoTracking()
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryResponseDto { CategoryId = c.CategoryId, Name = c.Name })
            .FirstOrDefaultAsync();
        return category is null ? NotFound() : category;
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryRequestDto request)
    {
        var name = request.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name == name))
            return Conflict(new { message = "The category already exists." });

        var category = new Category { Name = name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var response = new CategoryResponseDto { CategoryId = category.CategoryId, Name = category.Name };
        return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, response);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequestDto request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        var name = request.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.CategoryId != id && c.Name == name))
            return Conflict(new { message = "The category already exists." });

        category.Name = name;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return NotFound();
        if (await _context.MenuItems.AnyAsync(m => m.CategoryId == id))
            return Conflict(new { message = "The category contains menu items and cannot be deleted." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
