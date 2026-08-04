using RestaurantBookingSystem.DTOs.Restaurant;

namespace RestaurantBookingSystem.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<IReadOnlyList<RestaurantResponseDto>> GetAllAsync();

        Task<RestaurantResponseDto?> GetByIdAsync(int id);

        Task<RestaurantResponseDto> CreateAsync(RestaurantCreateDto dto);

        Task<bool> UpdateAsync(int id, RestaurantUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
