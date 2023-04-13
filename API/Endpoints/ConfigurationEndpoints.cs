using API.DTOs.Annotation;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class ConfigurationEndpoints
{
    private struct Categories
    {
        public string[] BackgroundCategories { get; set; }
        public string[] ContextCategories { get; set; }
        public TrashCategory[] TrashCategories { get; set; }
    }
    public static void MapConfigurationEndpoints(this WebApplication app)
    {
        app.MapGet("/configuration/categories", (IOptions<AppSettings> IappSettings) =>
        {

            Categories categories = new()
            {
                BackgroundCategories = IappSettings.Value.BackgroundCategories,
                ContextCategories = IappSettings.Value.ContextCategories,
                TrashCategories = IappSettings.Value.TrashCategories
            };
            return Results.Ok(categories);
        }).Produces<Categories>().AllowAnonymous();
    }
}