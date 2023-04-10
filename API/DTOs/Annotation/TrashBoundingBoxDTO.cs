using System.Drawing;
namespace API.DTOs.Annotation;
public class TrashBoundingBoxDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid TrashCountId { get; set; }
    public List<Rectangle> BoundingBoxs { get; set; } = null!;
}