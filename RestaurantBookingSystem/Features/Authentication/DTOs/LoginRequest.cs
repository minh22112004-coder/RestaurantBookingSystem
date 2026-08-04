using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Features.Authentication.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email or username is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
