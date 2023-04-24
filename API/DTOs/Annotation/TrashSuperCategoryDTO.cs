using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class TrashSuperCategoryDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid SubImageAnnotation { get; set; }
    public string TrashSuperCategoryLabel { get; set; }
}