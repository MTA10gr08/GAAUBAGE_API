using Swashbuckle.AspNetCore.Annotations;
namespace API.DTOs;
public class BaseDTO
{
    [SwaggerSchema(ReadOnly = true)] public Guid ID { get; set; }
    [SwaggerSchema(ReadOnly = true)] public DateTime Created { get; set; } = DateTime.Now;
    [SwaggerSchema(ReadOnly = true)] public DateTime Updated { get; set; } = DateTime.Now;
}
