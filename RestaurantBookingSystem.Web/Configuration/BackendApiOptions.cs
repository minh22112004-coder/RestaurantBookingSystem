using System.ComponentModel.DataAnnotations;

namespace RestaurantBookingSystem.Web.Configuration;

public sealed class BackendApiOptions
{
    public const string SectionName = "BackendApi";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
}
