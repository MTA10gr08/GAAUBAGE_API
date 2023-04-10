namespace API.DTOs.Annotation;
public class SegmentationDTO : BaseDTO
{
    public Guid UserId { get; set; }
    public Guid TrashBoundingBoxId { get; set; }
    public MultiPolygonDTO Segmentation { get; set; } = null!;
}