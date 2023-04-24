using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ImageAnnotationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid ImageID { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> BackgroundClassifications { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid? BackgroundClassificationConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> ContextClassifications { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid? ContextClassificationConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> SubImages { get; set; }
    [SwaggerSchema(ReadOnly = true)] public Guid? SubImagesConsensus { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsInProgress { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsComplete { get; set; }
}