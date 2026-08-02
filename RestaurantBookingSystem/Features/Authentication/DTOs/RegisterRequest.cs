using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Features.Authentication.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [MaxLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;
    }
}