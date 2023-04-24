using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SubImageAnnotationGroupDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid User { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    public ICollection<SubImageAnnotationDTO> SubImages { get; set; }
}