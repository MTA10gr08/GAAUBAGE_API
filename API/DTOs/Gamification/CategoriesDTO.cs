using Swashbuckle.AspNetCore.Annotations;

namespace API.DTOs.Annotation;
public class CategoriesDTO
{
    [SwaggerSchema(ReadOnly = true)] public string[] BackgroundCategories { get; set; }
    [SwaggerSchema(ReadOnly = true)] public string[] ContextCategories { get; set; }
    [SwaggerSchema(ReadOnly = true)] public TrashCategory[] TrashCategories { get; set; }
    public class TrashCategory
    {
        [SwaggerSchema(ReadOnly = true)] public string Name { get; set; }
        [SwaggerSchema(ReadOnly = true)] public string[] SubCategories { get; set; }
    }
}