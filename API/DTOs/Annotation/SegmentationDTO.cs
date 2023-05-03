using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SegmentationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Users { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid ImageAnnotation { get; set; }
    public MultiPolygonDTO SegmentationMultiPolygon { get; set; } = new MultiPolygonDTO();
}