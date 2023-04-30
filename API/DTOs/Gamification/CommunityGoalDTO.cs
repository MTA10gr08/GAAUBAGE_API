using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Gamification;
public class CommunityGoalDTO
{
    [SwaggerSchema(ReadOnly = true)] public string TaskType { get; set; } = string.Empty;
    [SwaggerSchema(ReadOnly = true)] public uint TotalToDo { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint DoneAll { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint DoneYou { get; set; }
}