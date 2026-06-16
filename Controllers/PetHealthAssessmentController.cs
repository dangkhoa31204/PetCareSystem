using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Models;
using PetCareSystem.API.Services.Gemini;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    /// <summary>
    /// API đánh giá sức khỏe thú cưng bằng Gemini AI
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetHealthAssessmentController : ControllerBase
    {
        private readonly PetCareSystemContext _context;
        private readonly IGeminiService _geminiService;

        public PetHealthAssessmentController(PetCareSystemContext context, IGeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        /// <summary>
        /// Gửi form đánh giá sức khỏe thú cưng - AI sẽ phân tích và trả về kết quả
        /// </summary>
        /// <param name="request">Form đánh giá sức khỏe</param>
        /// <returns>Kết quả đánh giá từ Gemini AI</returns>
        [HttpPost]
        public async Task<ActionResult<PetHealthAssessmentResponseDto>> AssessPetHealth(PetHealthAssessmentRequestDto request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            // Kiểm tra pet có thuộc về user không
            var pet = await _context.Pets
                .Where(p => p.PetId == request.PetId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound("Không tìm thấy thú cưng hoặc thú cưng không thuộc về bạn");

            // Gọi Gemini AI đánh giá
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            PetHealthAssessmentResponseDto result;

            try
            {
                result = await _geminiService.AssessPetHealthAsync(pet, request);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = "Lỗi kết nối tới dịch vụ AI", error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý phản hồi AI", error = ex.Message });
            }

            stopwatch.Stop();

            // Lưu log vào bảng LogAI
            var prompt = $"Đánh giá sức khỏe: {pet.Name} ({pet.Species}/{pet.Breed}) - " +
                         $"Hoạt động: {request.ActivityLevel}, Ăn uống: {request.AppetiteLevel}, " +
                         $"Nước: {request.WaterIntake}, Phân: {request.StoolCondition}, " +
                         $"Da/Lông: {request.SkinCoatCondition}" +
                         (!string.IsNullOrEmpty(request.Symptoms) ? $", Triệu chứng: {request.Symptoms}" : "");

            var responseText = $"Điểm: {result.OverallHealthScore}/10 ({result.HealthLevel}) - {result.Assessment}";

            var logAi = new LogAi
            {
                UserId = userId,
                PetId = pet.PetId,
                Prompt = prompt,
                Response = responseText,
                ModelName = "gemini-2.0-flash",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogAis.Add(logAi);
            await _context.SaveChangesAsync();

            return Ok(result);
        }

        /// <summary>
        /// Lấy lịch sử đánh giá sức khỏe AI của một thú cưng
        /// </summary>
        /// <param name="petId">ID thú cưng</param>
        /// <returns>Danh sách lịch sử đánh giá</returns>
        [HttpGet("history/{petId}")]
        public async Task<ActionResult> GetAssessmentHistory(long petId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            // Kiểm tra pet có thuộc về user không
            var petExists = await _context.Pets
                .AnyAsync(p => p.PetId == petId && p.UserId == userId);

            if (!petExists)
                return NotFound("Không tìm thấy thú cưng hoặc thú cưng không thuộc về bạn");

            var history = await _context.LogAis
                .Where(l => l.PetId == petId && l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.LogAiid,
                    l.Prompt,
                    l.Response,
                    l.ModelName,
                    l.ResponseTimeMs,
                    l.CreatedAt
                })
                .ToListAsync();

            return Ok(history);
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
