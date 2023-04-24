using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Gamification;
public class LeaderboardDTO
{
    [SwaggerSchema(ReadOnly = true)] public int CurrentUserSpot { get; set; }
    [SwaggerSchema(ReadOnly = true)] public ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public class Entry
    {
        [SwaggerSchema(ReadOnly = true)] public string Alias { get; set; } = string.Empty;
        [SwaggerSchema(ReadOnly = true)] public int Score { get; set; }
    }
}