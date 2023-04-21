using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SubImageAnnotationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid UserID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotationID { get; set; }
    public ICollection<BoundingBoxDTO> SubImages { get; set; }
}