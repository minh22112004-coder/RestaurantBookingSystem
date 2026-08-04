using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email or username.")]
    [Display(Name = "Email or username")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter a username.")]
    [StringLength(50, ErrorMessage = "The username cannot exceed 50 characters.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(100, ErrorMessage = "The email address cannot exceed 100 characters.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [StringLength(20, ErrorMessage = "The phone number cannot exceed 20 characters.")]
    [Display(Name = "Phone number")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please enter a password.")]
    [MinLength(6, ErrorMessage = "The password must contain at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The password confirmation does not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed record ProfileViewModel(
    int UserId,
    string Username,
    string Email,
    string Role);
