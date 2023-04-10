namespace API.DTOs.Annotation;
public class ContextCassificationDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid BackgroundClassificationId { get; set; }
    public string Category { get; set; } = string.Empty;
}