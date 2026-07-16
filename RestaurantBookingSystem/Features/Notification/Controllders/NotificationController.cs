using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Features.Notification.Services;

namespace RestaurantBookingSystem.Features.Notification.Controllders
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost]
        public IActionResult Create(Models.Notification notification)
        {
            var created = _notificationService.Create(notification);

            return Ok(created);
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var success = _notificationService.MarkAsRead(id);

            if (!success)
                return NotFound();

            return Ok("Notification marked as read.");
        }

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
