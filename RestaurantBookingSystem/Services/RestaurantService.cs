using RestaurantBookingSystem.DTOs.Restaurant;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Services
{
    public class RestaurantService : IRestaurantService
    {
        private static List<Restaurantfc2> restaurants = new()
    {
        new Restaurantfc2
        {
            RestaurantId = 1,
            Name = "Pizza Hut",
            Address = "Thu Dau Mot",
            Phone = "0909123456",
            OpenTime = new TimeOnly(8,0),
            CloseTime = new TimeOnly(22,0)
        }
    };

        public RestaurantResponseDto Create(RestaurantCreateDto dto)
        {
            var restaurant = new Restaurantfc2
            {
                RestaurantId = restaurants.Any()
                    ? restaurants.Max(r => r.RestaurantId) + 1
                    : 1,

                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime
            };

            restaurants.Add(restaurant);

            return new RestaurantResponseDto
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                OpenTime = restaurant.OpenTime,
                CloseTime = restaurant.CloseTime
            };
        }

        public bool Delete(int id)
        {
            var restaurant = restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
                return false;

            restaurants.Remove(restaurant);

            return true;
        }

        public IEnumerable<RestaurantResponseDto> GetAll()
        {
            return restaurants.Select(r => new RestaurantResponseDto
            {
                RestaurantId = r.RestaurantId,
                Name = r.Name,
                Address = r.Address,
                Phone = r.Phone,
                OpenTime = r.OpenTime,
                CloseTime = r.CloseTime
            });
        }

        public RestaurantResponseDto? GetById(int id)
        {
            var restaurant = restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
                return null;

            return new RestaurantResponseDto
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                OpenTime = restaurant.OpenTime,
                CloseTime = restaurant.CloseTime
            };
        }

        public bool Update(int id, RestaurantUpdateDto dto)
        {
            var restaurant = restaurants.FirstOrDefault(r => r.RestaurantId == id);

            if (restaurant == null)
                return false;

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            restaurant.OpenTime = dto.OpenTime;
            restaurant.CloseTime = dto.CloseTime;

            return true;
        }
    }
}
