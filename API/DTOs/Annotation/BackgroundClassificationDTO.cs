using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class BackgroundClassificationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    public ICollection<string> BackgroundClassificationLabels { get; set; } = new List<string>();
}