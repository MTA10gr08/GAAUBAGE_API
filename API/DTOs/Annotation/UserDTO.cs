using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Annotation;
public class UserDTO : BaseDTO
{
    [Required] public string Alias { get; set; } = string.Empty;
    [BindProperty(SupportsGet = false)] public string Tag { get; set; } = string.Empty;
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserContextCategoryIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserBackgroundContextIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserTrashCountIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserTrashBoundingBoxIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserTrashSuperCategoryIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserTrashCategoryIds { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> UserSegmentationIds { get; set; } = new List<Guid>();
}