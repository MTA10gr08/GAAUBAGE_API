using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ImageDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid User { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> SubImageAnnotations { get; set; } = new HashSet<Guid>();
    public string URI { get; set; } = string.Empty;
}