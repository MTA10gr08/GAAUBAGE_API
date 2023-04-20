using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Annotation;
public class DatabaseInfoDTO
{
    [SwaggerSchema(ReadOnly = true)] public uint TotalImages { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalBackgroundClassified { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalContextClassified { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashBoundingBoxed { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashSuperCategorised { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashCategorised { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSegmentated { get; set; }
}