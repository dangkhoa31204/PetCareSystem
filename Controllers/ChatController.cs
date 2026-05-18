using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Enums;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetCareSystem.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public ChatController(PetCareSystemContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        /// <summary>
        /// Gửi tin nhắn vào một cuộc trò chuyện
        /// </summary>
        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageDto dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return NotFound("Conversation not found.");
            }

            // Check if the current user is part of the conversation
            if (userRole == "Customer" && conversation.CustomerId != userId)
            {
                return Forbid("You are not part of this conversation.");
            }
            if (userRole == "Doctor" && conversation.DoctorId != userId)
            {
                return Forbid("You are not part of this conversation.");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                SenderType = userRole == "Doctor" ? (int)MessageSenderType.Doctor : (int)MessageSenderType.Customer,
                Content = dto.Content,
                MessageType = (int)MessageType.Text, // Assuming text for now
                SentAt = System.DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // In a real-time app, you would push this message via SignalR here
            return Ok(message);
        }

        /// <summary>
        /// Lấy lịch sử tin nhắn của một cuộc trò chuyện
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return NotFound("Conversation not found.");
            }

            // Check if the current user is part of the conversation
            if (userRole == "Customer" && conversation.CustomerId != userId)
            {
                return Forbid("You are not part of this conversation.");
            }
            if (userRole == "Doctor" && conversation.DoctorId != userId)
            {
                return Forbid("You are not part of this conversation.");
            }

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages.OrderBy(m => m.SentAt)); // Return in chronological order
        }

        /// <summary>
        /// Lấy danh sách các cuộc trò chuyện của người dùng hiện tại
        /// </summary>
        [HttpGet("my-conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            IQueryable<Conversation> query;

            if (userRole == "Doctor")
            {
                query = _context.Conversations.Where(c => c.DoctorId == userId);
            }
            else // Customer
            {
                query = _context.Conversations.Where(c => c.CustomerId == userId);
            }

            var conversations = await query
                .Include(c => c.Booking)
                .Include(c => c.Customer)
                .Include(c => c.Doctor)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Map to a DTO to avoid circular references and expose only needed data
            var conversationDtos = conversations.Select(c => new
            {
                c.ConversationId,
                c.BookingId,
                BookingInfo = c.Booking?.Note, // Or some other identifier
                CustomerId = c.CustomerId,
                CustomerName = c.Customer?.FullName,
                DoctorId = c.DoctorId,
                DoctorName = c.Doctor?.FullName,
                c.Status,
                c.CreatedAt
            });

            return Ok(conversationDtos);
        }
    }

    public class SendMessageDto
    {
        public string Content { get; set; }
    }
}
