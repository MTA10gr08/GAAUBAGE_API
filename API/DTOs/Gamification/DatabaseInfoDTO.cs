using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs.Annotation;
public class DatabaseInfoDTO
{
    [SwaggerSchema(ReadOnly = true)] public uint TotalImages { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSkipped { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalBackgroundClassifications { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalBackgroundClassified { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalContextClassifications { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalContextClassified { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSubImageGroups { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSubImageGrouped { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSubImageAnnotations { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashSuperCategorisations { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashSuperCategorised { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashSubCategorisations { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalTrashSubCategorised { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSegmentations { get; set; }
    [SwaggerSchema(ReadOnly = true)] public uint TotalSegmentated { get; set; }
}