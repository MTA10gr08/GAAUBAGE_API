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
    public string Alias { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public ICollection<ImageEntity> Images { get; set; } = new HashSet<ImageEntity>();
    public ICollection<ImageAnnotationEntity> VoteSkipped { get; set; } = new HashSet<ImageAnnotationEntity>();
    public ICollection<BackgroundClassificationEntity> BackgroundClassifications { get; set; } = new HashSet<BackgroundClassificationEntity>();
    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ICollection<SubImageAnnotationGroupEntity> SubImageAnnotationGroups { get; set; } = new HashSet<SubImageAnnotationGroupEntity>();
    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();

    public uint Score
    {
        get
        {
            var score = BackgroundClassifications
            .GroupBy(e => e.Created.Date.AddHours(-12))
            .Where(g => g.Count() > 10)
            .Select(g => g.Key)
            .Count() * 5;
            score += SubImageAnnotationGroups
            .GroupBy(e => e.Created.Date.AddHours(-12))
            .Where(g => g.Count() > 10)
            .Select(g => g.Key)
            .Count() * 5;
            score += TrashSubCategories
            .GroupBy(e => e.Created.Date.AddHours(-12))
            .Where(g => g.Count() > 5)
            .Select(g => g.Key)
            .Count() * 5;
            score += Segmentations
            .GroupBy(e => e.Created.Date.AddHours(-12))
            .Where(g => g.Count() > 5)
            .Select(g => g.Key)
            .Count() * 5;
            score += (BackgroundClassifications.Count * 1) + (SubImageAnnotationGroups.Count * 2) + (TrashSubCategories.Count * 3) + (Segmentations.Count * 4);

            return (uint)score;
        }
    }
    public uint Level
    {
        get
        {
            return (uint)(Score > 10 ? 1 : Score > 40 ? 2 : Score > 100 ? 3 : Score > 200 ? 4 : 0);
        }
    }
}

public class ImageEntity : BaseEntity
{
    public Guid UserID { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; } = null!;

    public ICollection<SubImageAnnotationEntity> SubImageAnnotations { get; set; } = new HashSet<SubImageAnnotationEntity>();

    public string URI { get; set; } = string.Empty;
}

public class ImageAnnotationEntity : BaseEntity
{
    public Guid ImageID { get; set; }
    public ImageEntity Image { get; set; } = null!;

    public ICollection<UserEntity> VoteSkipped { get; set; } = new HashSet<UserEntity>();

    public ICollection<BackgroundClassificationEntity> BackgroundClassifications { get; set; } = new HashSet<BackgroundClassificationEntity>();
    public BackgroundClassificationEntity? BackgroundClassificationConsensus
    {
        get
        {
            int total = BackgroundClassifications.Sum(x => x.Users.Count);
            double threshold = total * 0.5;
            return total > 3 ? BackgroundClassifications.FirstOrDefault(x => x.Users.Count >= threshold) : null;
        }
    }

    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ContextClassificationEntity? ContextClassificationConsensus
    {
        get
        {
            int total = ContextClassifications.Sum(x => x.Users.Count);
            double threshold = total * 0.5;
            return total > 3 ? ContextClassifications.FirstOrDefault(x => x.Users.Count >= threshold) : null;
        }
    }

    public ICollection<SubImageAnnotationGroupEntity> SubImageAnnotationGroups { get; set; } = new HashSet<SubImageAnnotationGroupEntity>();
    public SubImageAnnotationGroupEntity? SubImageAnnotationGroupConsensus
    {
        get
        {
            int total = SubImageAnnotationGroups.Sum(x => x.Users.Count);
            double threshold = total * 0.5;
            return total > 3 ? SubImageAnnotationGroups.FirstOrDefault(x => x.Users.Count >= threshold) : null;
        }
    }

    public bool IsInProgress => !(BackgroundClassificationConsensus && ContextClassificationConsensus)
        && (BackgroundClassifications.Any() || ContextClassifications.Any());
    public bool IsComplete => BackgroundClassificationConsensus && ContextClassificationConsensus;
    public bool IsSkipped
    {
        get
        {
            var skipped = VoteSkipped.Count;
            var annotations = BackgroundClassifications
                .SelectMany(x => x.Users)
                .Concat(ContextClassifications.SelectMany(x => x.Users))
                .Concat(SubImageAnnotationGroups.SelectMany(x => x.Users))
                .Distinct()
                .Count();
            if (skipped + annotations < 5) return false;
            return skipped > annotations;
        }
    }
}

public class BackgroundClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; } = null!;
    public ICollection<BackgroundClassificationStringEntity> BackgroundClassificationStrings { get; set; } = new HashSet<BackgroundClassificationStringEntity>();
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class BackgroundClassificationStringEntity : BaseEntity
{
    public string value { get; set; } = null!;
    public Guid BackgroundClassificationID { get; set; }
    public BackgroundClassificationEntity BackgroundClassification { get; set; } = null!;
    public static implicit operator string(BackgroundClassificationStringEntity r) => r.value;
}

public class ContextClassificationEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; } = null!;
    public string ContextClassification { get; set; } = null!;
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
}

public class SubImageAnnotationGroupEntity : BaseEntity
{
    public Guid ImageAnnotationID { get; set; }
    public ImageAnnotationEntity ImageAnnotation { get; set; } = null!;
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public ICollection<SubImageAnnotationEntity> SubImageAnnotations { get; set; } = new HashSet<SubImageAnnotationEntity>();
}

public class SubImageAnnotationEntity : BaseEntity
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public Guid ImageID { get; set; }
    public ImageEntity Image { get; set; } = null!;

    public Guid SubImageAnnotationGroupID { get; set; }
    public SubImageAnnotationGroupEntity SubImageAnnotationGroup { get; set; } = null!;

    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public TrashSuperCategoryEntity? TrashSuperCategoriesConsensus
    {
        get
        {
            int total = TrashSuperCategories.Sum(x => x.Users.Count);
            double threshold = total * 0.5;
            return total > 3 ? TrashSuperCategories.FirstOrDefault(x => x.Users.Count >= threshold) : null;
        }
    }
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public TrashSubCategoryEntity? TrashSubCategoriesConsensus
    {
        get
        {
            int total = TrashSubCategories.Sum(x => x.Users.Count);
            double threshold = total * 0.5;
            return total > 3 ? TrashSubCategories.FirstOrDefault(x => x.Users.Count >= threshold) : null;
        }
    }

    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();

    public bool IsInProgress => !(TrashSuperCategoriesConsensus && TrashSubCategoriesConsensus)
        && (TrashSuperCategories.Any() || TrashSubCategories.Any());
    public bool IsComplete => TrashSuperCategoriesConsensus && TrashSubCategoriesConsensus && Segmentations.Count > 3;
}

public class TrashSuperCategoryEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; } = null!;
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public string TrashSuperCategory { get; set; } = null!;
}

public class TrashSubCategoryEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; } = null!;
    public ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    public string TrashSubCategory { get; set; } = null!;

}

public class SegmentationEntity : BaseEntity
{
    public Guid SubImageAnnotationID { get; set; }
    public SubImageAnnotationEntity SubImageAnnotation { get; set; } = null!;
    public Guid UserID { get; set; }
    public UserEntity User { get; set; } = null!;
    public MultiPolygon Segmentation { get; set; } = null!;
}