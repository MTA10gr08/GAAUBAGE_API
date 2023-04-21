using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class BoundingBoxDTO : BaseDTO
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
}