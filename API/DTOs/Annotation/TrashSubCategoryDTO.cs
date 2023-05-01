using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class TrashSubCategoryDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid SubImageAnnotation { get; set; }
    public string TrashSubCategoryLabel { get; set; } = string.Empty;
}