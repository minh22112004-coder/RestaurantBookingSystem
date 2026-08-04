using System.ComponentModel.DataAnnotations;
using RestaurantBookingSystem.Web.Models;
using Xunit;

namespace RestaurantBookingSystem.Web.Tests;

public sealed class QualityValidationTests
{
    [Fact]
    public void RegisterViewModel_RejectsInvalidEmailShortPasswordAndMismatch()
    {
        var model = new RegisterViewModel
        {
            Username = "Customer",
            Email = "invalid-email",
            Password = "123",
            ConfirmPassword = "different"
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterViewModel.Password)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }

    [Fact]
    public void ReservationForm_RejectsPastDateAndEndBeforeStart()
    {
        var model = new ReservationFormViewModel
        {
            TableId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            StartTime = new TimeOnly(20, 0),
            EndTime = new TimeOnly(18, 0),
            GuestCount = 2
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ReservationFormViewModel.Date)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ReservationFormViewModel.EndTime)));
    }

    [Fact]
    public void RestaurantForm_RequiresClosingTimeAfterOpeningTime()
    {
        var model = new AdminRestaurantFormViewModel
        {
            Name = "Test Restaurant",
            Address = "123 Test Street",
            Phone = "0281234567",
            OpenTime = new TimeOnly(22, 0),
            CloseTime = new TimeOnly(8, 0)
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(AdminRestaurantFormViewModel.CloseTime)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void TableForm_RejectsCapacityOutsideSupportedRange(int capacity)
    {
        var model = new AdminTableFormViewModel
        {
            RestaurantId = 1,
            TableNumber = "T01",
            Capacity = capacity,
            Status = "Available"
        };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(AdminTableFormViewModel.Capacity)));
    }

    [Fact]
    public void MenuItemForm_RejectsZeroPriceAndMissingReferences()
    {
        var model = new AdminMenuItemFormViewModel { Name = "Test item", Price = 0 };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(AdminMenuItemFormViewModel.RestaurantId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(AdminMenuItemFormViewModel.CategoryId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(AdminMenuItemFormViewModel.Price)));
    }

    [Fact]
    public void ValidAdminForms_PassValidation()
    {
        var restaurant = new AdminRestaurantFormViewModel
        {
            Name = "Valid Restaurant", Address = "45 Market Street", Phone = "0281234567",
            OpenTime = new TimeOnly(8, 0), CloseTime = new TimeOnly(22, 0)
        };
        var item = new AdminMenuItemFormViewModel
        {
            RestaurantId = 1, CategoryId = 1, Name = "Valid item", Price = 10000, Available = true
        };

        Assert.Empty(Validate(restaurant));
        Assert.Empty(Validate(item));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
