using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Gamification;
public class LeaderboardDTO
{
    [SwaggerSchema(ReadOnly = true)] ICollection<string> Aliases { get; set; } = new List<string>();
}