using NetTopologySuite.Geometries;
namespace API.Entities;
public class GTSegmentationEntity : BaseGTProcessingEntity<UserSegmentationEntity, GTSegmentationEntity, MultiPolygon>
{
    public GTSegmentationEntity() : base() {}
    public Guid GTTrashCategoryId { get; set; }
    public GTTrashCategoryEntity GTTrashCategory { get; set; } = null!;
}
public class UserSegmentationEntity : BaseUserProcessingEntity<UserSegmentationEntity, GTSegmentationEntity, MultiPolygon>
{
    public UserSegmentationEntity() : base() {}
    public UserSegmentationEntity(MultiPolygon Data) : base() => this.Data = Data;
}