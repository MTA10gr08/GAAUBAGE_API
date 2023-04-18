using Microsoft.VisualBasic;
using NetTopologySuite.Geometries;

namespace API.EntitiesTest;
public abstract class BaseEntity
{
    public BaseEntity() { }
    public Guid ID { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.Now;
    public static implicit operator bool(BaseEntity? d) => d != null;
}

public class UserEntity : BaseEntity
{
    public string Alias { get; set; }
    public string Tag { get; set; }
    public ICollection<ImageEntity> Images { get; set; } = new HashSet<ImageEntity>();
    public ICollection<BackgroundClassificationLabelEntity> BackgroundClassificationLabels { get; set; } = new HashSet<BackgroundClassificationLabelEntity>();
    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ICollection<SubImageGroupEntity> SubImageGroups { get; set; } = new HashSet<SubImageGroupEntity>();
    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();
}

public class ImageEntity : BaseEntity
{
    public Guid UserID { get; set; }
    public UserEntity? User { get; set; }

    public Guid ImageAnnotationId { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }

    public string URI { get; set; }
}

public class ImageAnnotationEntity : BaseEntity
{
    public Guid ImageID { get; set; }
    public ImageEntity Image { get; set; }

    public Guid? BackgroundClassificationConsensusId { get; set; }
    public BackgroundClassificationEntity? BackgroundClassificationConsensus { get; set; }
    public ICollection<BackgroundClassificationEntity> BackgroundClassifications { get; set; } = new HashSet<BackgroundClassificationEntity>();

    public Guid? ContextClassificationConsensusId { get; set; }
    public ContextClassificationEntity? ContextClassificationConsensus { get; set; }
    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();

    public ICollection<SubImageAnnotationEntity> SubImagesConsensus { get; set; } = new HashSet<SubImageAnnotationEntity>();
    public ICollection<SubImageGroupEntity> SubImages { get; set; } = new HashSet<SubImageGroupEntity>();
}

public class BackgroundClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public ICollection<BackgroundClassificationLabelEntity> BackgroundClassificationLabels { get; set; } = new HashSet<BackgroundClassificationLabelEntity>();
}

public class BackgroundClassificationLabelEntity : BaseEntity
{
    public Guid BackgroundClassificationID { get; set; }
    public BackgroundClassificationEntity BackgroundClassification { get; set; }
    public string BackgroundClassificationLabel { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class ContextClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public string ContextClassification { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class BoundingBoxEntity : BaseEntity
{
    public Guid SubImageGroupID { get; set; }
    public SubImageGroupEntity? SubImageGroup { get; set; }
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity? SubImageAnnotation { get; set; }

    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
}

public class SubImageGroupEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public Guid UserID { get; set; }
    public UserEntity User { get; set; }
    public ICollection<BoundingBoxEntity> SubImages { get; set; } = new HashSet<BoundingBoxEntity>();
}

public class SubImageAnnotationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }

    public Guid SubImageID { get; set; }
    public BoundingBoxEntity SubImage { get; set; }

    public Guid? TrashSuperCategoryConsensusID { get; set; }
    public TrashSuperCategoryEntity? TrashSuperCategoryConsensus { get; set; }
    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();

    public Guid? TrashSubCategoryConsensusID { get; set; }
    public TrashSubCategoryEntity? TrashSubCategoryConsensus { get; set; }
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();

    public Guid? SegmentationConsensusID { get; set; }
    public SegmentationEntity? SegmentationConsensus { get; set; }
    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();
}

public class TrashSuperCategoryEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public string TrashSuperCategory { get; set; }
}

public class TrashSubCategoryEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public string TrashSubCategory { get; set; }

}

public class SegmentationEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; }
    public Guid UserID { get; set; }
    public UserEntity User { get; set; }
    public MultiPolygon Segmentation { get; set; } // MultiPolygon from NetTopologySuite.Geometries;
}