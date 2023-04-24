using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SegmentationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    public MultiPolygonDTO Segmentation { get; set; }
}