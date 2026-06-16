using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Models;

namespace PetCareSystem.API.Services.Gemini
{
    public interface IGeminiService
    {
        /// <summary>
        /// Đánh giá sức khỏe thú cưng bằng Gemini AI
        /// </summary>
        /// <param name="pet">Thông tin thú cưng từ database</param>
        /// <param name="request">Form đánh giá từ người dùng</param>
        /// <returns>Kết quả đánh giá sức khỏe</returns>
        Task<PetHealthAssessmentResponseDto> AssessPetHealthAsync(Pet pet, PetHealthAssessmentRequestDto request);
    }
}
