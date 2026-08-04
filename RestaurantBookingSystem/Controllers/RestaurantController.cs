using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.DTOs.Restaurant;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _restaurantService.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurant = await _restaurantService.GetByIdAsync(id);
        return restaurant is null ? NotFound() : Ok(restaurant);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RestaurantCreateDto dto)
    {
        try
        {
            var restaurant = await _restaurantService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = restaurant.RestaurantId }, restaurant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantUpdateDto dto)
    {
        try
        {
            return await _restaurantService.UpdateAsync(id, dto) ? NoContent() : NotFound();
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
            return await _restaurantService.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "The restaurant has related data and cannot be deleted." });
        }
    }
}
