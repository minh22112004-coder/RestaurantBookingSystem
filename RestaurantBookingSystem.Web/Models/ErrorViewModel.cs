namespace RestaurantBookingSystem.Web.Models;

public sealed class ErrorViewModel
{
    public int StatusCode { get; init; } = 500;
    public string Title { get; init; } = "Something went wrong";
    public string Message { get; init; } = "The request could not be completed. Please try again.";
    public string? RequestId { get; init; }
}
