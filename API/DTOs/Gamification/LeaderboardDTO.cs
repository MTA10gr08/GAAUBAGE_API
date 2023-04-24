using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Gamification;
public class LeaderboardDTO
{
    [SwaggerSchema(ReadOnly = true)] public uint CurrentUserSpot { get; set; }
    [SwaggerSchema(ReadOnly = true)] public ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public class Entry
    {
        [SwaggerSchema(ReadOnly = true)] public string Alias { get; set; } = string.Empty;
        [SwaggerSchema(ReadOnly = true)] public uint Score { get; set; }
    }
}