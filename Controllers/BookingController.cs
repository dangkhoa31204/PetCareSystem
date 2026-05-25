using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public BookingController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Customer] Lấy danh sách tất cả các lịch hẹn của người dùng hiện tại
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var bookingDtos = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingResponseDto
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    PetId = b.PetId,
                    PetName = b.Pet != null ? b.Pet.Name : string.Empty,
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Status = b.Status,
                    Note = b.Note,
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt,
                    Services = b.BookingDetails.Select(bd => new BookingDetailDto
                    {
                        BookingDetailId = bd.BookingDetailId,
                        ServiceId = bd.ServiceId,
                        ServiceName = bd.Service != null ? bd.Service.Name : string.Empty,
                        Quantity = bd.Quantity,
                        UnitPrice = bd.UnitPrice,
                        SubTotal = bd.SubTotal,
                        Note = bd.Note
                    }).ToList()
                })
                .ToListAsync();

            return Ok(bookingDtos);
        }

        /// <summary>
        /// [Customer] Lấy chi tiết một lịch hẹn bằng ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var booking = await _context.Bookings
                .Where(b => b.BookingId == id && b.UserId == userId)
                .Select(b => new BookingResponseDto
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    PetId = b.PetId,
                    PetName = b.Pet != null ? b.Pet.Name : string.Empty,
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Status = b.Status,
                    Note = b.Note,
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt,
                    Services = b.BookingDetails.Select(bd => new BookingDetailDto
                    {
                        BookingDetailId = bd.BookingDetailId,
                        ServiceId = bd.ServiceId,
                        ServiceName = bd.Service != null ? bd.Service.Name : string.Empty,
                        Quantity = bd.Quantity,
                        UnitPrice = bd.UnitPrice,
                        SubTotal = bd.SubTotal,
                        Note = bd.Note
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound("Booking not found");

            return Ok(booking);
        }

        /// <summary>
        /// [Customer] Tạo một lịch hẹn mới. Trạng thái ban đầu là 'Pending'.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(CreateBookingDto createBookingDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify pet belongs to user
            var pet = await _context.Pets
                .Where(p => p.PetId == createBookingDto.PetId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return BadRequest("Pet not found");

            // Verify all services exist
            var serviceIds = createBookingDto.Services.Select(s => s.ServiceId).ToList();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.ServiceId) && s.IsActive == true)
                .Select(s => new { s.ServiceId, s.Price })
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                return BadRequest("One or more services not found or inactive");

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                PetId = createBookingDto.PetId,
                BookingCode = GenerateBookingCode(),
                BookingDate = createBookingDto.BookingDate,
                StartTime = createBookingDto.StartTime,
                EndTime = createBookingDto.EndTime,
                Status = (int)BookingStatus.Pending,
                Note = createBookingDto.Note,
                CreatedAt = DateTime.UtcNow
            };

            // Add booking details
            decimal totalPrice = 0;
            foreach (var item in createBookingDto.Services)
            {
                var service = services.First(s => s.ServiceId == item.ServiceId);
                var bookingDetail = new BookingDetail
                {
                    ServiceId = item.ServiceId,
                    Quantity = item.Quantity,
                    UnitPrice = service.Price,
                    SubTotal = service.Price * item.Quantity
                };
                booking.BookingDetails.Add(bookingDetail);
                totalPrice += bookingDetail.SubTotal;
            }

            // Check if user is pro member and apply discount
            var activeMembership = _context.UserMemberships
                .Where(m => m.UserId == userId && 
                           m.Status == (int)Enums.UserMembershipStatus.Active &&
                           m.EndDate > DateTime.Now)
                .FirstOrDefault();

            if (activeMembership != null)
            {
                totalPrice = totalPrice * 0.85m; // 15% discount for pro members on services
            }

            booking.TotalPrice = totalPrice;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, MapToBookingResponseDto(booking));
        }

        /// <summary>
        /// [Customer] Cập nhật một lịch hẹn (chỉ khi trạng thái là 'Pending')
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(long id, CreateBookingDto updateBookingDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var booking = await _context.Bookings
                .Where(b => b.BookingId == id && b.UserId == userId)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound("Booking not found");

            if (booking.Status != (int)BookingStatus.Pending)
                return BadRequest("Only pending bookings can be updated");

            booking.BookingDate = updateBookingDto.BookingDate;
            booking.StartTime = updateBookingDto.StartTime;
            booking.EndTime = updateBookingDto.EndTime;
            booking.Note = updateBookingDto.Note;
            booking.UpdatedAt = DateTime.UtcNow;

            // Update booking details
            var serviceIds = updateBookingDto.Services.Select(s => s.ServiceId).ToList();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.ServiceId) && s.IsActive == true)
                .Select(s => new { s.ServiceId, s.Price })
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                return BadRequest("One or more services not found or inactive");

            // Remove old details
            _context.BookingDetails.RemoveRange(booking.BookingDetails);

            // Add new details
            decimal totalPrice = 0;
            foreach (var item in updateBookingDto.Services)
            {
                var service = services.First(s => s.ServiceId == item.ServiceId);
                var bookingDetail = new BookingDetail
                {
                    BookingId = booking.BookingId,
                    ServiceId = item.ServiceId,
                    Quantity = item.Quantity,
                    UnitPrice = service.Price,
                    SubTotal = service.Price * item.Quantity
                };
                _context.BookingDetails.Add(bookingDetail);
                totalPrice += bookingDetail.SubTotal;
            }

            booking.TotalPrice = totalPrice;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Ok("Booking updated successfully");
        }

        /// <summary>
        /// [Customer] Hủy một lịch hẹn (chỉ khi trạng thái là 'Pending')
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(long id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var booking = await _context.Bookings
                .Where(b => b.BookingId == id && b.UserId == userId)
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound("Booking not found");

            if (booking.Status != (int)BookingStatus.Pending)
                return BadRequest("Only pending bookings can be cancelled");

            booking.Status = (int)BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Ok("Booking cancelled successfully");
        }

        private BookingResponseDto MapToBookingResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                PetId = booking.PetId,
                PetName = booking.Pet?.Name ?? string.Empty,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                Note = booking.Note,
                TotalPrice = booking.TotalPrice,
                CreatedAt = booking.CreatedAt,
                Services = booking.BookingDetails?.Select(bd => new BookingDetailDto
                {
                    BookingDetailId = bd.BookingDetailId,
                    ServiceId = bd.ServiceId,
                    ServiceName = bd.Service?.Name ?? string.Empty,
                    Quantity = bd.Quantity,
                    UnitPrice = bd.UnitPrice,
                    SubTotal = bd.SubTotal,
                    Note = bd.Note
                }).ToList() ?? new List<BookingDetailDto>()
            };
        }

        private string GenerateBookingCode()
        {
            return "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        }

        private long GetUserIdFromClaims()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !long.TryParse(accountIdClaim.Value, out var accountId))
            {
                return 0;
            }

            return _context.Users
                .Where(u => u.AccountId == accountId)
                .Select(u => u.UserId)
                .FirstOrDefault();
        }
    }
}
