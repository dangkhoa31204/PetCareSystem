using System.Collections.Generic;

namespace PetCareSystem.API.Dtos.Customer
{
    public class PetHealthQuizDto
    {
        public long PetId { get; set; }
        public string QuizCategoryId { get; set; } = string.Empty;
        public string QuizCategoryTitle { get; set; } = string.Empty;
        public List<QuizAnswerDto> Answers { get; set; } = new();
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public int ScorePercent { get; set; }
        public QuizResultDto Result { get; set; } = new();
        public List<string> RecommendedProductTags { get; set; } = new();
        public List<QuizRecommendedProductDto> RecommendedProducts { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedOptionLabel { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class QuizResultDto
    {
        public string Level { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Advice { get; set; } = string.Empty;
    }

    public class QuizRecommendedProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
