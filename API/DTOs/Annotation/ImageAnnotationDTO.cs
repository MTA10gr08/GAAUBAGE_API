using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ImageAnnotationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid Image { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Skipped { get; set; }  = new HashSet<Guid>();

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> BackgroundClassifications { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? BackgroundClassificationConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> ContextClassifications { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? ContextClassificationConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> SubImages { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? SubImagesConsensus { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsInProgress { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsComplete { get; set; }
}