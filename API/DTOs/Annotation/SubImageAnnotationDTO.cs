using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class SubImageAnnotationDTO : BaseDTO
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    [SwaggerSchema(ReadOnly = true)] public Guid Image { get; set; }

    [SwaggerSchema(ReadOnly = true)] public Guid? SubImageAnnotationGroup { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSuperCategories { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? TrashSuperCategoriesConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSubCategories { get; set; } = new HashSet<Guid>();
    [SwaggerSchema(ReadOnly = true)] public Guid? TrashSubCategoriesConsensus { get; set; }

    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Segmentations { get; set; } = new HashSet<Guid>();

    [SwaggerSchema(ReadOnly = true)] public bool IsInProgress { get; set; }
    [SwaggerSchema(ReadOnly = true)] public bool IsComplete { get; set; }
}