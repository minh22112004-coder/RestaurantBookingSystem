using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Authentication.Services;

public interface ITokenService
{
    string CreateToken(
        User user,
        IList<string> roles
    );

    DateTime GetExpirationTime();
}