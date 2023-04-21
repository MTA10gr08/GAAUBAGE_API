using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs;
public class BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid ID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public DateTimeOffset Created { get; set; } = DateTime.Now;
    [SwaggerSchema(ReadOnly = true)] public DateTimeOffset Updated { get; set; } = DateTime.Now;
}
