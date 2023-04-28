using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Gamification;
public class UserGoalDTO
{
    [SwaggerSchema(ReadOnly = true)] public string TaskType { get; set; } = string.Empty;
    [SwaggerSchema(ReadOnly = true)] public uint TotalToDo { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint Done { get; set; }
}