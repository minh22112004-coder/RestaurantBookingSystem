using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Authorization.Policies;
using RestaurantBookingSystem.Features.Notification.Services;

namespace RestaurantBookingSystem.Features.Notification.Controllders
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Mặc định: phải đăng nhập mới gọi được bất kỳ action nào trong controller này
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_notificationService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var notification = _notificationService.GetById(id);

            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        // Chỉ Manager hoặc Admin mới được tạo thông báo
        [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
        [HttpPost]
        public IActionResult Create(Models.Notification notification)
        {
            var created = _notificationService.Create(notification);

            return Ok(created);
        }

        // Chỉ Manager hoặc Admin mới được đánh dấu đã đọc
        [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var success = _notificationService.MarkAsRead(id);

            if (!success)
                return NotFound();

            return Ok("Notification marked as read.");
        }

        // Chỉ Admin mới được xóa
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _notificationService.Delete(id);

            if (!success)
                return NotFound();

            return Ok("Notification deleted.");
        }
    }
}