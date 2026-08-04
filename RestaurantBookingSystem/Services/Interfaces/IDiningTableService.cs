using RestaurantBookingSystem.DTOs.DiningTable;

namespace RestaurantBookingSystem.Services.Interfaces
{
    public interface IDiningTableService
    {
        Task<IReadOnlyList<DiningTableResponseDto>> GetAllAsync();

        Task<DiningTableResponseDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<DiningTableResponseDto>> GetByRestaurantAsync(int restaurantId);

        Task<DiningTableResponseDto> CreateAsync(DiningTableCreateDto dto);

        Task<bool> UpdateAsync(int id, DiningTableUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
