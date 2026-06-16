namespace PetCareSystem.API.Dtos.Customer
{
    public class PetHealthAssessmentRequestDto
    {
        /// <summary>
        /// ID thú cưng cần đánh giá sức khỏe
        /// </summary>
        public long PetId { get; set; }

        /// <summary>
        /// Mức độ hoạt động: "Rất ít", "Ít", "Bình thường", "Nhiều", "Rất nhiều"
        /// </summary>
        public string ActivityLevel { get; set; } = null!;

        /// <summary>
        /// Mức độ ăn uống: "Không ăn", "Ăn ít", "Bình thường", "Ăn nhiều"
        /// </summary>
        public string AppetiteLevel { get; set; } = null!;

        /// <summary>
        /// Mức uống nước: "Rất ít", "Ít", "Bình thường", "Nhiều"
        /// </summary>
        public string WaterIntake { get; set; } = null!;

        /// <summary>
        /// Tình trạng phân: "Bình thường", "Lỏng", "Cứng", "Có máu", "Có nhầy"
        /// </summary>
        public string StoolCondition { get; set; } = null!;

        /// <summary>
        /// Tình trạng da/lông: "Bình thường", "Rụng nhiều", "Ngứa", "Nổi mẩn", "Khô"
        /// </summary>
        public string SkinCoatCondition { get; set; } = null!;

        /// <summary>
        /// Thay đổi hành vi đặc biệt (tùy chọn)
        /// </summary>
        public string? BehaviorChanges { get; set; }

        /// <summary>
        /// Triệu chứng bất thường, VD: "ho, hắt hơi, nôn" (tùy chọn)
        /// </summary>
        public string? Symptoms { get; set; }

        /// <summary>
        /// Lần khám bác sĩ gần nhất (tùy chọn)
        /// </summary>
        public string? LastVetVisit { get; set; }

        /// <summary>
        /// Ghi chú thêm (tùy chọn)
        /// </summary>
        public string? AdditionalNotes { get; set; }
    }
}
