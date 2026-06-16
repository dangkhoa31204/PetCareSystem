using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PetCareSystem.API.Models;
using PetCareSystem.API.Enums;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PetCareSystem.API.Hubs
{
    /// <summary>
    /// SignalR Hub xử lý nhắn tin real-time giữa Customer và Doctor
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly PetCareSystemContext _context;

        public ChatHub(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Khi client kết nối, tự động join vào tất cả conversation groups của user
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            var userRole = GetCurrentUserRole();

            if (userId > 0)
            {
                // Join vào tất cả conversations mà user tham gia
                List<long> conversationIds;

                if (userRole == "Doctor")
                {
                    conversationIds = await _context.Conversations
                        .Where(c => c.DoctorId == userId && c.Status == (int)ConversationStatus.Open)
                        .Select(c => c.ConversationId)
                        .ToListAsync();
                }
                else
                {
                    conversationIds = await _context.Conversations
                        .Where(c => c.CustomerId == userId && c.Status == (int)ConversationStatus.Open)
                        .Select(c => c.ConversationId)
                        .ToListAsync();
                }

                foreach (var conversationId in conversationIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
                }

                // Join vào group riêng của user để nhận notification
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Client join vào một conversation cụ thể (khi mở chat window)
        /// </summary>
        public async Task JoinConversation(long conversationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return;

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return;

            var userRole = GetCurrentUserRole();

            // Kiểm tra user có quyền tham gia conversation không
            if (userRole == "Customer" && conversation.CustomerId != userId) return;
            if (userRole == "Doctor" && conversation.DoctorId != userId) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        /// <summary>
        /// Client rời khỏi một conversation
        /// </summary>
        public async Task LeaveConversation(long conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        /// <summary>
        /// Gửi tin nhắn real-time - lưu DB và broadcast cho conversation
        /// </summary>
        public async Task SendMessage(long conversationId, string content)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized");
                return;
            }

            var userRole = GetCurrentUserRole();

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found");
                return;
            }

            // Kiểm tra quyền
            if (userRole == "Customer" && conversation.CustomerId != userId)
            {
                await Clients.Caller.SendAsync("Error", "You are not part of this conversation");
                return;
            }
            if (userRole == "Doctor" && conversation.DoctorId != userId)
            {
                await Clients.Caller.SendAsync("Error", "You are not part of this conversation");
                return;
            }

            // Kiểm tra conversation còn mở
            if (conversation.Status == (int)ConversationStatus.Closed)
            {
                await Clients.Caller.SendAsync("Error", "Conversation is closed");
                return;
            }

            // Lấy thông tin sender
            var sender = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            // Lưu message vào DB
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                SenderType = userRole == "Doctor" ? (int)MessageSenderType.Doctor : (int)MessageSenderType.User,
                Content = content,
                MessageType = (int)Enums.MessageType.Text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast message tới tất cả clients trong conversation
            var messageDto = new
            {
                message.MessageId,
                message.ConversationId,
                message.SenderId,
                SenderName = sender?.FullName ?? "Unknown",
                SenderAvatar = sender?.AvatarUrl,
                message.SenderType,
                message.Content,
                message.MessageType,
                message.SentAt
            };

            await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", messageDto);
        }

        /// <summary>
        /// Gửi thông báo đang gõ
        /// </summary>
        public async Task SendTyping(long conversationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return;

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", new
            {
                ConversationId = conversationId,
                UserId = userId,
                UserName = sender?.FullName ?? "Unknown"
            });
        }

        /// <summary>
        /// Thông báo đã ngừng gõ
        /// </summary>
        public async Task SendStopTyping(long conversationId)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0) return;

            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserStopTyping", new
            {
                ConversationId = conversationId,
                UserId = userId
            });
        }

        private async Task<long> GetCurrentUserIdAsync()
        {
            var accountIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !long.TryParse(accountIdClaim.Value, out var accountId))
                return 0;

            return await _context.Users
                .Where(u => u.AccountId == accountId)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
        }

        private string GetCurrentUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }
    }
}
