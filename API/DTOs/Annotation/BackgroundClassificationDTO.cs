using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class BackgroundClassificationDTO : BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid UserId { get; }
    public Guid ImageId { get; set; }
    public string BackgroundCategory { get; set; } = string.Empty;
}