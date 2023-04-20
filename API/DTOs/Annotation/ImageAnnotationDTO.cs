using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class ImageAnnotationDTO : BaseDTO
{
    public Guid ImageID { get; set; }

    public ICollection<Guid> BackgroundClassifications { get; set; }
    public Guid? BackgroundClassificationConsensus;

    public ICollection<Guid> ContextClassifications { get; set; }
    public Guid? ContextClassificationConsensus;

    public ICollection<Guid> SubImages { get; set; }
    public ICollection<Guid> SubImagesConsensus { get; set; }
    public bool IsInProgress;
    public bool IsComplete;
}