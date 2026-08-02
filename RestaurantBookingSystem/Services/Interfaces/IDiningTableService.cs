using RestaurantBookingSystem.DTOs.DiningTable;

namespace RestaurantBookingSystem.Services.Interfaces
{
    public interface IDiningTableService
    {
        IEnumerable<DiningTableResponseDto> GetAll();

        DiningTableResponseDto? GetById(int id);

        IEnumerable<DiningTableResponseDto> GetByRestaurant(int restaurantId);

        DiningTableResponseDto Create(DiningTableCreateDto dto);

        bool Update(int id, DiningTableUpdateDto dto);

        bool Delete(int id);
    }
}