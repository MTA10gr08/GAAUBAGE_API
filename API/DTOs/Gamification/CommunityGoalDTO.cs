namespace API.DTOs.Gamification;
public class CommunityGoalDTO
{
    public string TaskType { get; set; } = string.Empty;
    public uint TotalToDo;
    public uint DoneAll;
    public uint DoneYou;
}