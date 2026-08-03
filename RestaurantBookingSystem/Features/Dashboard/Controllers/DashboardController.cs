using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Dashboard.Services;

namespace RestaurantBookingSystem.Features.Dashboard.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public class DashboardController : ControllerBase
    {
        private readonly IReportService _reportService;

        public DashboardController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int? restaurantId)
        {
            try
            {
                var result = await _reportService.GetOverviewAsync(restaurantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", error = ex.Message });
            }
        }
    }
}
