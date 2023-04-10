using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Gamification;
public class FeedbackDTO
{
    [SwaggerSchema(WriteOnly = true)] public string Feedback { get; set; } = string.Empty;
}