namespace API.DTOs.Annotation;
public class TrashSuperCategoryDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid TrashBoundingBoxId { get; set; }
    public string SuperCategory { get; set; } = string.Empty;
}