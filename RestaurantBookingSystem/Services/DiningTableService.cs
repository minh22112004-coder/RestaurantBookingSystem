using RestaurantBookingSystem.DTOs.DiningTable;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Services.Interfaces;

namespace RestaurantBookingSystem.Services
{
    public class DiningTableService : IDiningTableService
    {
        private static List<DiningTablefc2> tables = new()
        {
            new DiningTablefc2
            {
                TableId = 1,
                RestaurantId = 1,
                TableNumber = "A01",
                Capacity = 4,
                Status = "Available"
            },

            new DiningTablefc2
            {
                TableId = 2,
                RestaurantId = 1,
                TableNumber = "A02",
                Capacity = 2,
                Status = "Reserved"
            }
        };

        public IEnumerable<DiningTableResponseDto> GetAll()
        {
            return tables.Select(t => new DiningTableResponseDto
            {
                TableId = t.TableId,
                RestaurantId = t.RestaurantId,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Status = t.Status
            });
        }

        public DiningTableResponseDto? GetById(int id)
        {
            var table = tables.FirstOrDefault(t => t.TableId == id);

            if (table == null)
                return null;

            return new DiningTableResponseDto
            {
                TableId = table.TableId,
                RestaurantId = table.RestaurantId,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        public IEnumerable<DiningTableResponseDto> GetByRestaurant(int restaurantId)
        {
            return tables
                .Where(t => t.RestaurantId == restaurantId)
                .Select(t => new DiningTableResponseDto
                {
                    TableId = t.TableId,
                    RestaurantId = t.RestaurantId,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    Status = t.Status
                });
        }

        public DiningTableResponseDto Create(DiningTableCreateDto dto)
        {
            var table = new DiningTablefc2
            {
                TableId = tables.Any()
                    ? tables.Max(t => t.TableId) + 1
                    : 1,

                RestaurantId = dto.RestaurantId,
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity,
                Status = dto.Status
            };

            tables.Add(table);

            return new DiningTableResponseDto
            {
                TableId = table.TableId,
                RestaurantId = table.RestaurantId,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Status = table.Status
            };
        }

        public bool Update(int id, DiningTableUpdateDto dto)
        {
            var table = tables.FirstOrDefault(t => t.TableId == id);

            if (table == null)
                return false;

            table.RestaurantId = dto.RestaurantId;
            table.TableNumber = dto.TableNumber;
            table.Capacity = dto.Capacity;
            table.Status = dto.Status;

            return true;
        }

        public bool Delete(int id)
        {
            var table = tables.FirstOrDefault(t => t.TableId == id);

            if (table == null)
                return false;

            tables.Remove(table);

            return true;
        }
    }
}