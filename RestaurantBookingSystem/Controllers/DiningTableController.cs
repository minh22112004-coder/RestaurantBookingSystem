using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.DTOs.DiningTable;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiningTableController : ControllerBase
    {
        private readonly IDiningTableService _tableService;

        public DiningTableController(IDiningTableService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_tableService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var table = _tableService.GetById(id);

            if (table == null)
                return NotFound();

            return Ok(table);
        }

        [HttpGet("restaurant/{restaurantId}")]
        public IActionResult GetByRestaurant(int restaurantId)
        {
            return Ok(_tableService.GetByRestaurant(restaurantId));
        }

        [HttpPost]
        public IActionResult Create([FromBody] DiningTableCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = _tableService.Create(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = table.TableId },
                table);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DiningTableUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_tableService.Update(id, dto))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_tableService.Delete(id))
                return NotFound();

            return NoContent();
        }
    }
}