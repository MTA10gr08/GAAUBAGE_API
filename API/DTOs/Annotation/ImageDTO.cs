using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ImageDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid UserID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotationID { get; set; }
    public string URI { get; set; } = string.Empty;
}