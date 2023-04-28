using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;

namespace API.Endpoints;
public static class DatabaseInfoEndpoints
{
    public static void MapDatabaseInfoEndpoints(this WebApplication app)
    {
        app.MapGet("/databaseinfo", (DataContext dataContext) =>
        {
            return Results.Ok(new DatabaseInfoDTO
            {
                TotalImages = (uint)dataContext.Images.Count(),
                TotalSkipped = (uint)dataContext.ImageAnnotations.Select(x => x.VoteSkipped).Distinct().Count(),
                TotalSubImages = (uint)dataContext.SubImageGroups.Select(x => x.ImageAnnotation).Distinct().Select(x => x.SubImageAnnotationGroups.Select(y => y.SubImageAnnotations)).Count(),
                TotalBackgroundClassified = (uint)dataContext.BackgroundClassifications.Select(x => x.ImageAnnotation).Distinct().Count(),
                TotalContextClassified = (uint)dataContext.ContextClassifications.Select(x => x.ImageAnnotation).Distinct().Count(),
                TotalSubImaged = (uint)dataContext.SubImageGroups.Select(x => x.ImageAnnotation).Distinct().Count(),
                TotalTrashSuperCategorised = (uint)dataContext.TrashSuperCategories.Select(x => x.SubImageAnnotation).Distinct().Count(),
                TotalTrashSubCategorised = (uint)dataContext.TrashSubCategories.Select(x => x.SubImageAnnotation).Distinct().Count(),
                TotalSegmentated = (uint)dataContext.Segmentations.Select(x => x.SubImageAnnotation).Distinct().Count(),
            });
        }).Produces<DatabaseInfoDTO>();
    }
}