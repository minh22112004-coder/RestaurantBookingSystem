namespace RestaurantBookingSystem.Features.Authentication.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public UserResponse User { get; set; } = new();
    }

    public class UserResponse
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}