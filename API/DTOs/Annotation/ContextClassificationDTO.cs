using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ContextClassificationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid UserID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotationID { get; set; }
    public string ContextClassificationLabel { get; set; }
}