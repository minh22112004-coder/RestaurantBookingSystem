using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.DTOs.Restaurant;
using RestaurantBookingSystem.Services.Interfaces;

namespace Function_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_restaurantService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var restaurant = _restaurantService.GetById(id);

            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        [HttpPost]
        public IActionResult Create([FromBody] RestaurantCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var restaurant = _restaurantService.Create(dto);

            return CreatedAtAction(nameof(GetById),
                new { id = restaurant.RestaurantId },
                restaurant);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] RestaurantUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_restaurantService.Update(id, dto))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_restaurantService.Delete(id))
                return NotFound();

            return NoContent();
        }
    }
}