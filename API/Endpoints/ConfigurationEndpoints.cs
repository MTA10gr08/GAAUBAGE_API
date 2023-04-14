using API.DTOs.Annotation;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static partial class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this WebApplication app)
    {
        app.MapGet("/configuration/categories", (IOptions<AppSettings> IappSettings) =>
        {

            CategoriesDTO categories = new()
            {
                BackgroundCategories = IappSettings.Value.BackgroundCategories,
                ContextCategories = IappSettings.Value.ContextCategories,
                TrashCategories = IappSettings.Value.TrashCategories.Select(x => new CategoriesDTO.TrashCategory
                {
                    Name = x.Name,
                    SubCategories = x.SubCategories
                }).ToArray()
            };
            return Results.Ok(categories);
        }).Produces<CategoriesDTO>().AllowAnonymous();
    }
}