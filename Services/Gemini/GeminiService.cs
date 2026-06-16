using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetCareSystem.API.Services.Gemini
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PetHealthAssessmentResponseDto> AssessPetHealthAsync(Pet pet, PetHealthAssessmentRequestDto request)
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            var model = _configuration["GeminiAI:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Gemini API Key chưa được cấu hình trong appsettings.json");

            var prompt = BuildPrompt(pet, request);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.PostAsync(url, httpContent);
            stopwatch.Stop();

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseString);
                throw new HttpRequestException($"Gemini API trả về lỗi: {response.StatusCode}");
            }

            var result = ParseGeminiResponse(responseString, pet);
            result.AssessedAt = DateTime.UtcNow;

            return result;
        }

        private string BuildPrompt(Pet pet, PetHealthAssessmentRequestDto request)
        {
            // Tính tuổi pet
            string ageInfo = "Không rõ";
            if (pet.BirthDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - pet.BirthDate.Value.Year;
                var months = today.Month - pet.BirthDate.Value.Month;
                if (months < 0) { age--; months += 12; }
                ageInfo = age > 0 ? $"{age} tuổi {months} tháng" : $"{months} tháng tuổi";
            }

            var sb = new StringBuilder();

            sb.AppendLine("Bạn là một bác sĩ thú y AI chuyên nghiệp. Hãy đánh giá sức khỏe thú cưng dựa trên thông tin sau và trả về kết quả dưới dạng JSON.");
            sb.AppendLine();
            sb.AppendLine("=== THÔNG TIN THÚ CƯNG ===");
            sb.AppendLine($"- Tên: {pet.Name}");
            sb.AppendLine($"- Loài: {pet.Species ?? "Không rõ"}");
            sb.AppendLine($"- Giống: {pet.Breed ?? "Không rõ"}");
            sb.AppendLine($"- Tuổi: {ageInfo}");
            sb.AppendLine($"- Giới tính: {(pet.Gender == 1 ? "Đực" : pet.Gender == 2 ? "Cái" : "Không rõ")}");
            sb.AppendLine($"- Cân nặng hiện tại: {(pet.CurrentWeight.HasValue ? $"{pet.CurrentWeight} kg" : "Không rõ")}");
            sb.AppendLine($"- Đã triệt sản: {(pet.IsNeutered == true ? "Có" : "Không")}");
            sb.AppendLine($"- Tình trạng sức khỏe ghi nhận: {pet.HealthStatus ?? "Chưa có"}");
            sb.AppendLine($"- Thông tin dị ứng: {pet.AllergyInfo ?? "Không có"}");
            sb.AppendLine($"- Thông tin tiêm phòng: {pet.VaccinationInfo ?? "Chưa có"}");
            sb.AppendLine($"- Tiền sử bệnh: {pet.MedicalHistory ?? "Chưa có"}");
            sb.AppendLine();
            sb.AppendLine("=== ĐÁNH GIÁ TỪ CHỦ NUÔI ===");
            sb.AppendLine($"- Mức độ hoạt động: {request.ActivityLevel}");
            sb.AppendLine($"- Mức độ ăn uống: {request.AppetiteLevel}");
            sb.AppendLine($"- Mức uống nước: {request.WaterIntake}");
            sb.AppendLine($"- Tình trạng phân: {request.StoolCondition}");
            sb.AppendLine($"- Tình trạng da/lông: {request.SkinCoatCondition}");

            if (!string.IsNullOrWhiteSpace(request.BehaviorChanges))
                sb.AppendLine($"- Thay đổi hành vi: {request.BehaviorChanges}");

            if (!string.IsNullOrWhiteSpace(request.Symptoms))
                sb.AppendLine($"- Triệu chứng bất thường: {request.Symptoms}");

            if (!string.IsNullOrWhiteSpace(request.LastVetVisit))
                sb.AppendLine($"- Lần khám gần nhất: {request.LastVetVisit}");

            if (!string.IsNullOrWhiteSpace(request.AdditionalNotes))
                sb.AppendLine($"- Ghi chú thêm: {request.AdditionalNotes}");

            sb.AppendLine();
            sb.AppendLine("=== YÊU CẦU ===");
            sb.AppendLine("Hãy phân tích và trả về JSON với cấu trúc sau (trả về ĐÚNG format JSON, không thêm markdown):");
            sb.AppendLine(@"{
  ""overallHealthScore"": <số nguyên từ 1 đến 10>,
  ""healthLevel"": ""<Tốt | Khá | Trung bình | Cần chú ý | Cần khám ngay>"",
  ""assessment"": ""<đánh giá chi tiết bằng tiếng Việt, 3-5 câu>"",
  ""recommendations"": [""<khuyến nghị 1>"", ""<khuyến nghị 2>"", ...],
  ""warningSignsDetected"": [""<dấu hiệu cảnh báo 1>"", ...],
  ""suggestedServices"": [""<dịch vụ gợi ý 1>"", ...]
}");
            sb.AppendLine();
            sb.AppendLine("Lưu ý:");
            sb.AppendLine("- overallHealthScore: 8-10 = Tốt, 6-7 = Khá, 4-5 = Trung bình, 2-3 = Cần chú ý, 1 = Cần khám ngay");
            sb.AppendLine("- recommendations: tối thiểu 2, tối đa 5 khuyến nghị cụ thể và thực tế");
            sb.AppendLine("- warningSignsDetected: có thể rỗng nếu không phát hiện dấu hiệu cảnh báo");
            sb.AppendLine("- suggestedServices: gợi ý các dịch vụ như 'Khám tổng quát', 'Xét nghiệm máu', 'Tiêm phòng', 'Tắm spa', 'Cắt tỉa lông', 'Khám da liễu', 'Khám nội khoa', 'Tẩy giun'");
            sb.AppendLine("- Trả lời hoàn toàn bằng tiếng Việt");

            return sb.ToString();
        }

        private PetHealthAssessmentResponseDto ParseGeminiResponse(string responseString, Pet pet)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                // Gemini response structure: candidates[0].content.parts[0].text
                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException("Gemini trả về response rỗng");

                // Clean markdown code block nếu có
                text = text.Trim();
                if (text.StartsWith("```json"))
                    text = text[7..];
                if (text.StartsWith("```"))
                    text = text[3..];
                if (text.EndsWith("```"))
                    text = text[..^3];
                text = text.Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var aiResult = JsonSerializer.Deserialize<GeminiHealthResult>(text, options);

                if (aiResult == null)
                    throw new InvalidOperationException("Không thể parse kết quả từ Gemini");

                return new PetHealthAssessmentResponseDto
                {
                    PetName = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    OverallHealthScore = Math.Clamp(aiResult.OverallHealthScore, 1, 10),
                    HealthLevel = aiResult.HealthLevel ?? "Trung bình",
                    Assessment = aiResult.Assessment ?? "Không có đánh giá chi tiết",
                    Recommendations = aiResult.Recommendations ?? new List<string>(),
                    WarningSignsDetected = aiResult.WarningSignsDetected ?? new List<string>(),
                    SuggestedServices = aiResult.SuggestedServices ?? new List<string>()
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Lỗi parse Gemini response: {Response}", responseString);
                throw new InvalidOperationException("Không thể xử lý phản hồi từ Gemini AI", ex);
            }
        }

        /// <summary>
        /// Internal class để deserialize kết quả từ Gemini
        /// </summary>
        private class GeminiHealthResult
        {
            [JsonPropertyName("overallHealthScore")]
            public int OverallHealthScore { get; set; }

            [JsonPropertyName("healthLevel")]
            public string? HealthLevel { get; set; }

            [JsonPropertyName("assessment")]
            public string? Assessment { get; set; }

            [JsonPropertyName("recommendations")]
            public List<string>? Recommendations { get; set; }

            [JsonPropertyName("warningSignsDetected")]
            public List<string>? WarningSignsDetected { get; set; }

            [JsonPropertyName("suggestedServices")]
            public List<string>? SuggestedServices { get; set; }
        }
    }
}
