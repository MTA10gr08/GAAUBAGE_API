using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ContextClassificationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    public string ContextClassificationLabel { get; set; }
}