using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models; // Updated model key type
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

            var result = await TryUpdateBookingStatusAsync(booking, updateDto.Status);
            if (result != null)
            {
                return result;
            }

            return Ok("Booking status updated successfully");
        }

        /// <summary>
        /// Chỉ định bác sĩ cho một lịch hẹn
        /// </summary>
        [HttpPut("{bookingId}/assign-doctor")]
        public async Task<IActionResult> AssignDoctor(long bookingId, [FromBody] AssignDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            if (booking.Status == (int)BookingStatus.Cancelled || booking.Status == (int)BookingStatus.Completed)
            {
                return BadRequest("Cannot assign a doctor to a cancelled or completed booking.");
            }

            var doctor = await _context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.UserId == dto.DoctorId && u.Account.Role == (int)AccountRole.Doctor);
            if (doctor == null)
            {
                doctor = await _context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.AccountId == dto.DoctorId && u.Account.Role == (int)AccountRole.Doctor);
            }
            if (doctor == null)
            {
                return BadRequest("Invalid Doctor ID or the user is not a Doctor.");
            }

            if (booking.Status == (int)BookingStatus.Pending)
            {
                booking.Status = (int)BookingStatus.Confirmed;
            }
            booking.UpdatedAt = DateTime.UtcNow;

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c =>
                c.CustomerId == booking.UserId &&
                c.DoctorId == doctor.UserId &&
                c.PetId == booking.PetId &&
                c.Type == (int)ConversationType.Doctor &&
                c.Status == (int)ConversationStatus.Open);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    CustomerId = booking.UserId,
                    DoctorId = doctor.UserId,
                    PetId = booking.PetId,
                    Type = (int)ConversationType.Doctor,
                    StartedAt = DateTime.UtcNow,
                    Status = (int)ConversationStatus.Open
                };
                _context.Conversations.Add(conversation);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Doctor assigned successfully.",
                bookingId = booking.BookingId,
                doctorId = doctor.UserId,
                status = booking.Status,
                conversationId = conversation.ConversationId
            });
        }

        /// <summary>
        /// Lấy danh sách các bác sĩ có lịch trống trong một khoảng thời gian
        /// </summary>
        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            var doctors = await _context.Users
                .Include(u => u.Account)
                .Where(u => u.Account.Role == (int)AccountRole.Doctor)
                .Select(u => new { u.UserId, u.FullName, u.Specialization })
                .ToListAsync();

            return Ok(doctors);
        }

        /// <summary>
        /// Lấy danh sách tất cả bác sĩ (role = 3) để staff gán cho lịch hẹn
        /// </summary>
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors([FromQuery] string? search)
        {
            var keyword = search?.Trim().ToLower();

            var doctors = await _context.Database
                .SqlQueryRaw<DoctorDto>(@"
                    SELECT u.UserId, u.AccountId, u.FullName, u.Phone, a.Email, u.AvatarUrl,
                           NULL AS Specialization
                    FROM Users u
                    INNER JOIN Accounts a ON u.AccountId = a.AccountId
                    WHERE a.Role = 3
                ")
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                doctors = doctors.Where(d =>
                    d.FullName.ToLower().Contains(keyword) ||
                    (d.Specialization != null && d.Specialization.ToLower().Contains(keyword)) ||
                    (d.Phone != null && d.Phone.Contains(keyword))
                ).ToList();
            }

            return Ok(doctors.OrderBy(d => d.FullName));
        }

        private async Task<IActionResult?> TryUpdateBookingStatusAsync(Booking booking, int status, bool updateEndTime = false)
        {
            var currentStatus = (BookingStatus)booking.Status.GetValueOrDefault(0);
            var newStatus = (BookingStatus)status;

            var allowedTransitions = new Dictionary<BookingStatus, List<BookingStatus>>
            {
                { BookingStatus.Pending, new List<BookingStatus> { BookingStatus.Confirmed, BookingStatus.Cancelled } },
                { BookingStatus.Confirmed, new List<BookingStatus> { BookingStatus.InProgress, BookingStatus.Cancelled } },
                { BookingStatus.InProgress, new List<BookingStatus> { BookingStatus.Completed, BookingStatus.Cancelled } },
                { BookingStatus.Completed, new List<BookingStatus>() },
                { BookingStatus.Cancelled, new List<BookingStatus>() }
            };

            if (!allowedTransitions.ContainsKey(currentStatus) || !allowedTransitions[currentStatus].Contains(newStatus))
                return BadRequest($"Cannot transition from {currentStatus} to {newStatus}");

            booking.Status = status;
            if (updateEndTime)
            {
                booking.EndTime = DateTime.UtcNow;
            }

            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return null;
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
