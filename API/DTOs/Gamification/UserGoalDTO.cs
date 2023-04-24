namespace API.DTOs.Gamification;
public class UserGoalDTO
{
    public string TaskType { get; set; } = string.Empty;
    public uint TotalToDo;
    public uint Done;
}