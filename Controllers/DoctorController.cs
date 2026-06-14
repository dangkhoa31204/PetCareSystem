using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Enums;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using PetCareSystem.API.Dtos.Doctor;

namespace PetCareSystem.API.Controllers
{
    [Route("api/doctor")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public DoctorController(PetCareSystemContext context)
        {
            _context = context;
        }

        private async Task<long> GetDoctorIdAsync()
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(accountIdClaim, out var accountId))
            {
                return 0;
            }

            var doctorId = await _context.Users
                .Where(u => u.AccountId == accountId)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            return doctorId;
        }

        /// <summary>
        /// Lấy danh sách các lịch hẹn được chỉ định cho bác sĩ đang đăng nhập
        /// </summary>
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] BookingStatus? status)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == 0)
            {
                return Unauthorized();
            }
            var assignedPetIds = _context.Conversations
                .Where(c => c.DoctorId == doctorId &&
                            c.Type == (int)ConversationType.Doctor &&
                            c.Status == (int)ConversationStatus.Open)
                .Select(c => c.PetId);

            var query = _context.Bookings
                .Where(b => assignedPetIds.Contains(b.PetId))
                .Include(b => b.Pet)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == (int)status.Value);
            }

            // In a real app, mapping to a DTO is recommended
            var bookings = await query.ToListAsync();
            return Ok(bookings);
        }

        /// <summary>
        /// Bác sĩ cập nhật trạng thái của một lịch hẹn (e.g., Confirmed -> InProgress)
        /// </summary>
        [HttpPut("bookings/{bookingId}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, [FromBody] UpdateBookingStatusByDoctorDto dto)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == 0)
            {
                return Unauthorized();
            }
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var isAssigned = await _context.Conversations.AnyAsync(c =>
                c.DoctorId == doctorId &&
                c.CustomerId == booking.UserId &&
                c.PetId == booking.PetId &&
                c.Type == (int)ConversationType.Doctor &&
                c.Status == (int)ConversationStatus.Open);

            if (!isAssigned)
            {
                return NotFound("Booking not found or you are not assigned to this booking.");
            }

            var currentStatus = (BookingStatus)booking.Status;
            var newStatus = dto.Status;

            // Doctor can only change status from Confirmed to InProgress
            if (currentStatus != BookingStatus.Confirmed || newStatus != BookingStatus.InProgress)
            {
                return BadRequest($"Cannot transition from {currentStatus} to {newStatus}.");
            }

            booking.Status = (int)newStatus;
            booking.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Booking status updated to InProgress.");
        }

        /// <summary>
        /// Bác sĩ thêm ghi chú khám bệnh và hoàn tất lịch hẹn
        /// </summary>
        [HttpPost("bookings/{bookingId}/complete")]
        public async Task<IActionResult> CompleteBooking(int bookingId, [FromBody] AddClinicalNoteDto dto)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == 0)
            {
                return Unauthorized();
            }
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var isAssigned = await _context.Conversations.AnyAsync(c =>
                c.DoctorId == doctorId &&
                c.CustomerId == booking.UserId &&
                c.PetId == booking.PetId &&
                c.Type == (int)ConversationType.Doctor &&
                c.Status == (int)ConversationStatus.Open);

            if (!isAssigned)
            {
                return NotFound("Booking not found or you are not assigned to this booking.");
            }

            if ((BookingStatus)booking.Status != BookingStatus.InProgress)
            {
                return BadRequest("Booking must be InProgress to be completed.");
            }

            // Add the clinical note to the pet's record or a new table
            // For simplicity, we'll add it to the booking's note for now.
            // A dedicated PetHealthRecord table would be better.
            booking.Note += $"\n[Doctor's Note - {System.DateTime.UtcNow.ToString("g")}]: {dto.Note}";

            booking.Status = (int)BookingStatus.Completed;
            booking.EndTime = System.DateTime.UtcNow;
            booking.UpdatedAt = System.DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Booking completed and clinical note added.");
        }
    }
}
