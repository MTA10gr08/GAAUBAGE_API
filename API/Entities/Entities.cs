using Microsoft.VisualBasic;
using NetTopologySuite.Geometries;

namespace API.Entities;

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
    public ICollection<BackgroundClassificationEntity> BackgroundClassifications { get; set; } = new HashSet<BackgroundClassificationEntity>();
    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ICollection<SubImageAnnotationGroupEntity> SubImageAnnotationGroups { get; set; } = new HashSet<SubImageAnnotationGroupEntity>();
    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();
}

public class ImageEntity : BaseEntity
{
    public Guid UserID { get; set; }
    public UserEntity User { get; set; }

    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }

    public string URI { get; set; }
}

public class ImageAnnotationEntity : BaseEntity
{
    public Guid ImageID { get; set; }
    public ImageEntity Image { get; set; }

    public ICollection<BackgroundClassificationEntity> BackgroundClassifications { get; set; } = new HashSet<BackgroundClassificationEntity>();
    public BackgroundClassificationEntity? BackgroundClassificationConsensus
    {
        get
        {
            int total = BackgroundClassifications.Count;
            double threshold = total * 0.75;
            return BackgroundClassifications.FirstOrDefault(x => x.Users.Count >= threshold);
        }
    }

    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ContextClassificationEntity? ContextClassificationConsensus
    {
        get
        {
            int total = ContextClassifications.Count;
            double threshold = total * 0.75;
            return ContextClassifications.FirstOrDefault(x => x.Users.Count >= threshold);
        }
    }

    public ICollection<SubImageAnnotationGroupEntity> SubImageAnnotationGroups { get; set; } = new HashSet<SubImageAnnotationGroupEntity>();
    public SubImageAnnotationGroupEntity? SubImageAnnotationGroupConsensus
    {
        get
        {
            int total = SubImageAnnotationGroups.Count;
            double threshold = total * 0.75;
            return SubImageAnnotationGroups.FirstOrDefault(x => x.Users.Count >= threshold);
        }
    }

    public bool IsInProgress => !(BackgroundClassificationConsensus && ContextClassificationConsensus)
        && (BackgroundClassifications.Any() || ContextClassifications.Any());
    public bool IsComplete => BackgroundClassificationConsensus && ContextClassificationConsensus;
}

public class BackgroundClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public ICollection<BackgroundClassificationStringEntity> BackgroundClassificationStrings { get; set; } = new HashSet<BackgroundClassificationStringEntity>();
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class BackgroundClassificationStringEntity : BaseEntity
{
    public string value { get; set; }
    public Guid BackgroundClassificationID { get; set; }
    public BackgroundClassificationEntity BackgroundClassification { get; set; }
    public static implicit operator string(BackgroundClassificationStringEntity r) => r.value;
}

public class ContextClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public string ContextClassification { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class SubImageAnnotationGroupEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; }
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public ICollection<SubImageAnnotationEntity> SubImageAnnotations { get; set; } = new HashSet<SubImageAnnotationEntity>();
}

public class SubImageAnnotationEntity : BaseEntity
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public Guid SubImageAnnotationGroupID { get; set; }
    public SubImageAnnotationGroupEntity SubImageAnnotationGroup { get; set; }

    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
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