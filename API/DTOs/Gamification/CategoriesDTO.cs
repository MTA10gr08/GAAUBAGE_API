using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class CategoriesDTO
{
    [SwaggerSchema(ReadOnly = true)] public string[] BackgroundCategories { get; set; } = Array.Empty<string>();
    [SwaggerSchema(ReadOnly = true)] public string[] ContextCategories { get; set; } = Array.Empty<string>();
    [SwaggerSchema(ReadOnly = true)] public TrashCategory[] TrashCategories { get; set; } = Array.Empty<TrashCategory>();
    public class TrashCategory
    {
        [SwaggerSchema(ReadOnly = true)] public string Name { get; set; } = string.Empty;
        [SwaggerSchema(ReadOnly = true)] public string[] SubCategories { get; set; } = Array.Empty<string>();
    }
}