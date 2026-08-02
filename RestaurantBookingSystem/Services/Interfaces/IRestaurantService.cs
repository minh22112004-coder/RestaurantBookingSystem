using RestaurantBookingSystem.DTOs.Restaurant;

namespace RestaurantBookingSystem.Services.Interfaces
{
    public interface IRestaurantService
    {
        IEnumerable<RestaurantResponseDto> GetAll();

        RestaurantResponseDto? GetById(int id);

        RestaurantResponseDto Create(RestaurantCreateDto dto);

        bool Update(int id, RestaurantUpdateDto dto);

        bool Delete(int id);
    }
}
