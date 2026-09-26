using System.Text.Json.Serialization;

namespace EduqPlus.API.DTOs
{
    public class CourseFeatureDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text_content")]
        public string TextContent { get; set; } = string.Empty;

        [JsonPropertyName("trust_score")]
        public double TrustScore { get; set; }
    }

    public class RecommendationRequestDto
    {
        [JsonPropertyName("target_course_id")]
        public string TargetCourseId { get; set; } = string.Empty;

        [JsonPropertyName("courses")]
        public List<CourseFeatureDto> Courses { get; set; } = new();

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
