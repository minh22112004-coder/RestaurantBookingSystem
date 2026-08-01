using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Reservation.Services;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        public ReservationController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                bool result = await _reservationService.CreateReservationAsync(dto);
                if (result)
                {
                    return Ok(new { message = "Đặt bàn thành công!" });
                }
                return BadRequest(new { message = "Không thể tạo lịch đặt bàn." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }

        // GET: api/Reservation/customer/{userId}
        [HttpGet("customer/{userId}")]
        public async Task<ActionResult<IEnumerable<Models.Reservation>>> GetReservationsByCustomer(int userId)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByCustomerAsync(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }

        // GET: api/Reservation/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<Models.Reservation>>> GetReservationsByDate(DateOnly date)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsByDateAsync(date);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }

        // PUT: api/Reservation/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
        {
            try
            {
                bool result = await _reservationService.UpdateReservationAsync(id, dto);
                if (result)
                {
                    return Ok(new { message = "Cập nhật lịch đặt bàn thành công!" });
                }
                return BadRequest(new { message = "Không thể cập nhật lịch đặt bàn." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }

        // PUT: api/Reservation/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                bool result = await _reservationService.CancelReservationAsync(id);
                if (result)
                {
                    return Ok(new { message = "Đã hủy lịch đặt bàn thành công." });
                }
                return BadRequest(new { message = "Không thể hủy lịch đặt bàn." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }
    }
}