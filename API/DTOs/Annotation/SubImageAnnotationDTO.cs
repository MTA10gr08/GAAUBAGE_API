using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SubImageAnnotationDTO : BaseDTO
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    [SwaggerSchema(ReadOnly = true)] public Guid? SubImageAnnotationGroup { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSuperCategories { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? TrashSuperCategoriesConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSubCategories { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? TrashSubCategoriesConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Segmentations { get; set; } = new HashSet<Guid>();

    [SwaggerSchema(ReadOnly = true)] public bool IsInProgress { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsComplete { get; set; }
}
/* public class SubImageAnnotationGroupEntity : BaseEntity
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
    public TrashSuperCategoryEntity? TrashSuperCategoriesConsensus
    {
        get
        {
            int total = TrashSuperCategories.Count;
            double threshold = total * 0.75;
            return TrashSuperCategories.FirstOrDefault(x => x.Users.Count >= threshold);
        }
    }
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public TrashSubCategoryEntity? TrashSubCategoriesConsensus
    {
        get
        {
            int total = TrashSubCategories.Count;
            double threshold = total * 0.75;
            return TrashSubCategories.FirstOrDefault(x => x.Users.Count >= threshold);
        }
    }

    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();

    public bool IsInProgress => !(TrashSuperCategoriesConsensus && TrashSubCategoriesConsensus)
        && (TrashSuperCategories.Any() || TrashSubCategories.Any());
    public bool IsComplete => TrashSuperCategoriesConsensus && TrashSubCategoriesConsensus;
}*/