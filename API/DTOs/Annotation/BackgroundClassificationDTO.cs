using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class BackgroundClassificationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid UserID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotationID { get; set; }
    public ICollection<string> BackgroundClassificationLabels { get; set; }
}