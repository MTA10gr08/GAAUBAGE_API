namespace API.DTOs.Annotation;
public class TrashCountDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid ContextCassificationId { get; set; }
    public uint Count { get; set; }
}