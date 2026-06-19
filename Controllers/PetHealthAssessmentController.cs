using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PetCareSystem.API.Controllers
{
    /// <summary>
    /// API đánh giá sức khỏe thú cưng qua trắc nghiệm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PetHealthAssessmentController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public PetHealthAssessmentController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lưu kết quả bài trắc nghiệm sức khỏe thú cưng vào lịch sử LogAI
        /// </summary>
        /// <param name="request">Kết quả bài trắc nghiệm</param>
        /// <returns>Kết quả lưu Log thành công</returns>
        [HttpPost]
        public async Task<IActionResult> SaveQuizResult([FromBody] PetHealthQuizDto request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            // Kiểm tra pet có thuộc về user không
            var pet = await _context.Pets
                .Where(p => p.PetId == request.PetId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound("Không tìm thấy thú cưng hoặc thú cưng không thuộc về bạn");

            // Serialize prompt (câu hỏi/trả lời) và response (kết quả/sản phẩm) dạng JSON để FE dễ dàng parse
            var promptJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                quizCategoryId = request.QuizCategoryId,
                quizCategoryTitle = request.QuizCategoryTitle,
                totalScore = request.TotalScore,
                maxScore = request.MaxScore,
                scorePercent = request.ScorePercent,
                answers = request.Answers
            });

            var responseJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                result = request.Result,
                recommendedProductTags = request.RecommendedProductTags,
                recommendedProducts = request.RecommendedProducts
            });

            var logAi = new LogAi
            {
                UserId = userId,
                PetId = pet.PetId,
                Prompt = promptJson,
                Response = responseJson,
                ModelName = "PetHealthQuiz",
                ResponseTimeMs = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogAis.Add(logAi);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, logId = logAi.LogAiid, message = "Lưu kết quả trắc nghiệm thành công" });
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
