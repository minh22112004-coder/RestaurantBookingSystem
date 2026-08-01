using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Reservation.Services
{
    public class ReservationService
    {
        private readonly RestaurantReservationDbContext _context;

        public ReservationService(RestaurantReservationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTableAvailableAsync(int tableId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            var overlappingReservation = await _context.Reservations
                .Where(r => r.TableId == tableId && r.Date == date && r.Status != "Cancelled")
                .AnyAsync(r => r.StartTime < endTime && r.EndTime > startTime);

            return !overlappingReservation;
        }

        public async Task<bool> CreateReservationAsync(CreateReservationDto dto)
        {
            var table = await _context.DiningTables
                .Include(t => t.Restaurant)
                .FirstOrDefaultAsync(t => t.TableId == dto.TableId);

            if (table == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin bàn ăn.");
            }

            if (dto.GuestCount > table.Capacity)
            {
                throw new InvalidOperationException($"Số lượng khách vượt quá sức chứa của bàn (Tối đa {table.Capacity} người).");
            }

            if (table.Restaurant != null && table.Restaurant.OpenTime.HasValue && table.Restaurant.CloseTime.HasValue)
            {
                if (dto.StartTime < table.Restaurant.OpenTime.Value || dto.EndTime > table.Restaurant.CloseTime.Value)
                {
                    throw new InvalidOperationException($"Nhà hàng chỉ mở cửa từ {table.Restaurant.OpenTime} đến {table.Restaurant.CloseTime}.");
                }
            }

            bool isAvailable = await IsTableAvailableAsync(dto.TableId, dto.Date, dto.StartTime, dto.EndTime);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Bàn đã được đặt trong khung giờ này.");
            }

            var reservation = new Models.Reservation
            {
                UserId = dto.UserId,
                TableId = dto.TableId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                GuestCount = dto.GuestCount,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Models.Reservation>> GetReservationsByCustomerAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Reservation>> GetReservationsByDateAsync(DateOnly date)
        {
            return await _context.Reservations
                .Where(r => r.Date == date)
                .ToListAsync();
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin đặt bàn.");
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateReservationAsync(int reservationId, UpdateReservationDto dto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .ThenInclude(t => t.Restaurant)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin đặt bàn.");
            }

            var table = await _context.DiningTables
                .Include(t => t.Restaurant)
                .FirstOrDefaultAsync(t => t.TableId == dto.TableId);

            if (table == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin bàn ăn mới.");
            }

            if (dto.GuestCount > table.Capacity)
            {
                throw new InvalidOperationException($"Số lượng khách vượt quá sức chứa của bàn (Tối đa {table.Capacity} người).");
            }

            if (table.Restaurant != null && table.Restaurant.OpenTime.HasValue && table.Restaurant.CloseTime.HasValue)
            {
                if (dto.StartTime < table.Restaurant.OpenTime.Value || dto.EndTime > table.Restaurant.CloseTime.Value)
                {
                    throw new InvalidOperationException($"Nhà hàng chỉ mở cửa từ {table.Restaurant.OpenTime} đến {table.Restaurant.CloseTime}.");
                }
            }

            var overlappingReservation = await _context.Reservations
                .Where(r => r.TableId == dto.TableId && r.Date == dto.Date && r.ReservationId != reservationId && r.Status != "Cancelled")
                .AnyAsync(r => r.StartTime < dto.EndTime && r.EndTime > dto.StartTime);

            if (overlappingReservation)
            {
                throw new InvalidOperationException("Bàn đã được đặt trong khung giờ mới này.");
            }

            reservation.TableId = dto.TableId;
            reservation.Date = dto.Date;
            reservation.StartTime = dto.StartTime;
            reservation.EndTime = dto.EndTime;
            reservation.GuestCount = dto.GuestCount;

            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class CreateReservationDto
    {
        public int UserId { get; set; }
        public int TableId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int GuestCount { get; set; }
    }

    public class UpdateReservationDto
    {
        public int TableId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int GuestCount { get; set; }
    }
}