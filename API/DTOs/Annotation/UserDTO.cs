using System.Net.Mime;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Annotation;
public class UserDTO : BaseDTO
{
    [Required] public string Alias { get; set; } = string.Empty;
    [BindProperty(SupportsGet = false)] public string Tag { get; set; } = string.Empty;
    [SwaggerSchema(ReadOnly = true)] public uint Score { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint Level { get; set; }
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Images { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> BackgroundClassificationLabels { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> ContextClassifications { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> SubImageGroups { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSuperCategories { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSubCategories { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Segmentations { get; set; } = new List<Guid>();
}
/*
    public string Alias { get; set; }
    public string Tag { get; set; }
    public ICollection<ImageEntity> Images { get; set; } = new HashSet<ImageEntity>();
    public ICollection<BackgroundClassificationLabelEntity> BackgroundClassificationLabels { get; set; } = new HashSet<BackgroundClassificationLabelEntity>();
    public ICollection<ContextClassificationEntity> ContextClassifications { get; set; } = new HashSet<ContextClassificationEntity>();
    public ICollection<SubImageGroupEntity> SubImageGroups { get; set; } = new HashSet<SubImageGroupEntity>();
    public ICollection<TrashSuperCategoryEntity> TrashSuperCategories { get; set; } = new HashSet<TrashSuperCategoryEntity>();
    public ICollection<TrashSubCategoryEntity> TrashSubCategories { get; set; } = new HashSet<TrashSubCategoryEntity>();
    public ICollection<SegmentationEntity> Segmentations { get; set; } = new HashSet<SegmentationEntity>();
    */