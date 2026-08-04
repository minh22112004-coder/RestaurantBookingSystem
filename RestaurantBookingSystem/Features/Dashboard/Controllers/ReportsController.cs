using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Dashboard.Dtos;
using RestaurantBookingSystem.Features.Dashboard.Services;

namespace RestaurantBookingSystem.Features.Dashboard.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetRevenueReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetReservationReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetReservationReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("tables/occupancy")]
        public async Task<IActionResult> GetTableOccupancy([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetTableOccupancyReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("menu-items/top-selling")]
        public async Task<IActionResult> GetTopSellingMenuItems([FromQuery] ReportFilterDto filter, [FromQuery] int top = 10)
        {
            try
            {
                var result = await _reportService.GetTopSellingMenuItemsAsync(filter, top);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("menu-items/by-category")]
        public async Task<IActionResult> GetRevenueByCategory([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetRevenueByCategoryAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("customers/top")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] ReportFilterDto filter, [FromQuery] int top = 10)
        {
            try
            {
                var result = await _reportService.GetTopCustomersAsync(filter, top);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }

        [HttpGet("customers/new")]
        public async Task<IActionResult> GetNewCustomers([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetNewCustomersReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A system error occurred.", error = ex.Message });
            }
        }
    }
}
