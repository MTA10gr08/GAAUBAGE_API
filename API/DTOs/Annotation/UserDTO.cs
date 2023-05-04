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
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Skipped { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> BackgroundClassifications { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> ContextClassifications { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> SubImageGroups { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSuperCategories { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> TrashSubCategories { get; set; } = new List<Guid>();
    [SwaggerSchema(ReadOnly = true)] public ICollection<Guid> Segmentations { get; set; } = new List<Guid>();
}