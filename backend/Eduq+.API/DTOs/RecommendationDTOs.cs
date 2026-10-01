using System.Text.Json.Serialization;

namespace EduqPlus.API.DTOs
{
    public class RecommendationRequestDto
    {
        [JsonPropertyName("target_course_id")]
        public string TargetCourseId { get; set; } = string.Empty;

        [JsonPropertyName("k")]
        public int K { get; set; } = 5;
    }

    public class RecommendationResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("recommendations")]
        public List<RecommendationItemDto> Recommendations { get; set; } = new();
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class RecommendationItemDto
    {
        [JsonPropertyName("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}
