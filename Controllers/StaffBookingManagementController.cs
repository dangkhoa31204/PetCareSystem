using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Staff;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/staff/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class BookingManagementController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public BookingManagementController(PetCareSystemContext context)
        {
            _context = context;
        }

        // GET: api/staff/bookingmanagement - Get all bookings with filter
        [HttpGet]
        public async Task<ActionResult<List<BookingDetailForStaffDto>>> GetAllBookings(
            [FromQuery] int? status = null,
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 10)
        {
            var pageNum = pageNumber ?? 1;
            var pageSz = pageSize ?? 10;

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Pet)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Service)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((pageNum - 1) * pageSz)
                .Take(pageSz)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => MapToBookingDetailForStaffDto(b)).ToList();
            return Ok(bookingDtos);
        }

        // GET: api/staff/bookingmanagement/{bookingId}
        [HttpGet("{bookingId}")]
        public async Task<ActionResult<BookingDetailForStaffDto>> GetBookingDetail(long bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Pet)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Booking not found");

            return Ok(MapToBookingDetailForStaffDto(booking));
        }

        // GET: api/staff/bookingmanagement/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BookingDetailForStaffDto>>> GetUserBookings(long userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.Pet)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Service)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => MapToBookingDetailForStaffDto(b)).ToList();
            return Ok(bookingDtos);
        }

        // GET: api/staff/bookingmanagement/pet/{petId}
        [HttpGet("pet/{petId}")]
        public async Task<ActionResult<List<BookingDetailForStaffDto>>> GetPetBookings(long petId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.PetId == petId)
                .Include(b => b.User)
                .Include(b => b.Pet)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Service)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => MapToBookingDetailForStaffDto(b)).ToList();
            return Ok(bookingDtos);
        }

        // PUT: api/staff/bookingmanagement/{bookingId}/status
        [HttpPut("{bookingId}/status")]
        public async Task<IActionResult> UpdateBookingStatus(long bookingId, UpdateBookingStatusDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Booking not found");

            // Validate status transition
            var currentStatus = (BookingStatus)booking.Status.GetValueOrDefault(0);
            var newStatus = (BookingStatus)updateDto.Status;

            // Allowed transitions
            var allowedTransitions = new Dictionary<BookingStatus, List<BookingStatus>>
            {
                { BookingStatus.Pending, new List<BookingStatus> { BookingStatus.Confirmed, BookingStatus.Cancelled } },
                { BookingStatus.Confirmed, new List<BookingStatus> { BookingStatus.InProgress, BookingStatus.Cancelled } },
                { BookingStatus.InProgress, new List<BookingStatus> { BookingStatus.Completed, BookingStatus.Cancelled } },
                { BookingStatus.Completed, new List<BookingStatus> { } },
                { BookingStatus.Cancelled, new List<BookingStatus> { } }
            };

            if (!allowedTransitions.ContainsKey(currentStatus) || !allowedTransitions[currentStatus].Contains(newStatus))
                return BadRequest($"Cannot transition from {currentStatus} to {newStatus}");

            booking.Status = updateDto.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Ok("Booking status updated successfully");
        }

        // PUT: api/staff/bookingmanagement/{bookingId}/complete
        [HttpPut("{bookingId}/complete")]
        public async Task<IActionResult> CompleteBooking(long bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound("Booking not found");

            if (booking.Status != (int)BookingStatus.InProgress)
                return BadRequest("Only in-progress bookings can be completed");

            booking.Status = (int)BookingStatus.Completed;
            booking.EndTime = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Ok("Booking completed successfully");
        }

        /// <summary>
        /// Chỉ định bác sĩ cho một lịch hẹn
        /// </summary>
        [HttpPut("{bookingId}/assign-doctor")]
        public async Task<IActionResult> AssignDoctor(long bookingId, [FromBody] AssignDoctorDto dto)
        {
            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var doctor = await _context.Users
                                     .Include(u => u.Account)
                                     .FirstOrDefaultAsync(u => u.UserId == dto.DoctorId && u.Account.Role == (int)AccountRole.Doctor);
            if (doctor == null)
            {
                return BadRequest("Invalid Doctor ID or the user is not a Doctor.");
            }

            booking.DoctorId = dto.DoctorId;
            booking.UpdatedAt = DateTime.UtcNow;

            // Create a conversation for the booking
            var conversation = new Conversation
            {
                BookingId = booking.BookingId,
                CustomerId = booking.UserId,
                DoctorId = dto.DoctorId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)ConversationStatus.Active
            };
            _context.Conversations.Add(conversation);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor assigned successfully.", conversationId = conversation.ConversationId });
        }

        /// <summary>
        /// Lấy danh sách các bác sĩ có lịch trống trong một khoảng thời gian
        /// </summary>
        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            // Find doctors who have conflicting bookings in the given time slot
            var conflictingDoctorIds = await _context.Bookings
                .Where(b => b.DoctorId.HasValue &&
                            b.Status != (int)BookingStatus.Cancelled &&
                            b.Status != (int)BookingStatus.Completed &&
                            ((startTime < b.EndTime && endTime > b.StartTime)))
                .Select(b => b.DoctorId.Value)
                .Distinct()
                .ToListAsync();

            // Get all doctors and filter out the ones with conflicts
            var availableDoctors = await _context.Users
                .Include(u => u.Account)
                .Where(u => u.Account.Role == (int)AccountRole.Doctor && !conflictingDoctorIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.FullName, u.Specialization }) // Add other relevant fields
                .ToListAsync();

            return Ok(availableDoctors);
        }

        private BookingDetailForStaffDto MapToBookingDetailForStaffDto(Booking booking)
        {
            return new BookingDetailForStaffDto
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                UserId = booking.UserId,
                OwnerName = booking.User?.FullName ?? string.Empty,
                PetId = booking.PetId,
                PetName = booking.Pet?.Name ?? string.Empty,
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                Note = booking.Note,
                TotalPrice = booking.TotalPrice,
                CreatedAt = booking.CreatedAt,
                Services = booking.BookingDetails?.Select(bd => new BookingServiceDto
                {
                    BookingDetailId = bd.BookingDetailId,
                    ServiceId = bd.ServiceId,
                    ServiceName = bd.Service?.Name ?? string.Empty,
                    Quantity = bd.Quantity,
                    UnitPrice = bd.UnitPrice,
                    SubTotal = bd.SubTotal,
                    Note = bd.Note
                }).ToList() ?? new List<BookingServiceDto>()
            };
        }
    }
}
