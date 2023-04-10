namespace API.DTOs.Annotation;
public class TrashCategoryDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid TrashSuperCategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
}