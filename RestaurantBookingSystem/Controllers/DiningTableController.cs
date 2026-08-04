using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.DiningTable;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiningTableController : ControllerBase
{
    private readonly IDiningTableService _tableService;

    public DiningTableController(IDiningTableService tableService)
    {
        _tableService = tableService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _tableService.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var table = await _tableService.GetByIdAsync(id);
        return table is null ? NotFound() : Ok(table);
    }

    [AllowAnonymous]
    [HttpGet("restaurant/{restaurantId:int}")]
    public async Task<IActionResult> GetByRestaurant(int restaurantId) =>
        Ok(await _tableService.GetByRestaurantAsync(restaurantId));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DiningTableCreateDto dto)
    {
        try
        {
            var table = await _tableService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = table.TableId }, table);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DiningTableUpdateDto dto)
    {
        try
        {
            return await _tableService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            return await _tableService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "The table has reservation history and cannot be deleted." });
        }
    }
}
