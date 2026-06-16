namespace PetCareSystem.API.Dtos.Customer
{
    public class PetHealthAssessmentResponseDto
    {
        /// <summary>
        /// Tên thú cưng
        /// </summary>
        public string PetName { get; set; } = null!;

        /// <summary>
        /// Loài thú cưng
        /// </summary>
        public string? Species { get; set; }

        /// <summary>
        /// Giống
        /// </summary>
        public string? Breed { get; set; }

        /// <summary>
        /// Điểm sức khỏe tổng thể (1-10)
        /// </summary>
        public int OverallHealthScore { get; set; }

        /// <summary>
        /// Mức sức khỏe: "Tốt", "Khá", "Trung bình", "Cần chú ý", "Cần khám ngay"
        /// </summary>
        public string HealthLevel { get; set; } = null!;

        /// <summary>
        /// Đánh giá chi tiết từ AI
        /// </summary>
        public string Assessment { get; set; } = null!;

        /// <summary>
        /// Danh sách khuyến nghị chăm sóc
        /// </summary>
        public List<string> Recommendations { get; set; } = new();

        /// <summary>
        /// Dấu hiệu cảnh báo phát hiện được
        /// </summary>
        public List<string> WarningSignsDetected { get; set; } = new();

        /// <summary>
        /// Dịch vụ gợi ý từ hệ thống
        /// </summary>
        public List<string> SuggestedServices { get; set; } = new();

        /// <summary>
        /// Thời gian đánh giá
        /// </summary>
        public DateTime AssessedAt { get; set; }
    }
}
