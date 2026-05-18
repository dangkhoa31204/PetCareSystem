using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PetCareSystem.API.Controllers
{
    [Route("api/staff/feedbackmanagement")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Staff and Admin can view feedback
    public class StaffFeedbackManagementController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public StaffFeedbackManagementController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả các phản hồi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFeedbacks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Booking)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new 
                {
                    f.FeedbackId,
                    f.UserId,
                    UserName = f.User.FullName,
                    f.BookingId,
                    f.Rating,
                    f.Comment,
                    f.CreatedAt
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        /// <summary>
        /// Lấy chi tiết một phản hồi
        /// </summary>
        [HttpGet("{feedbackId}")]
        public async Task<IActionResult> GetFeedback(int feedbackId)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Booking)
                .Where(f => f.FeedbackId == feedbackId)
                .Select(f => new 
                {
                    f.FeedbackId,
                    f.UserId,
                    UserName = f.User.FullName,
                    f.BookingId,
                    f.Rating,
                    f.Comment,
                    f.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (feedback == null)
            {
                return NotFound("Feedback not found");
            }

            return Ok(feedback);
        }
    }
}
