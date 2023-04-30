using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;

namespace API.Endpoints;
public static class DatabaseInfoEndpoints
{
    public static void MapDatabaseInfoEndpoints(this WebApplication app)
    {
        /*app.MapGet("/databaseinfo", (DataContext dataContext) =>
        {
            return Results.Ok(new DatabaseInfoDTO
            {
                TotalImages = (uint)dataContext.Images.Count(),
                TotalSkipped = (uint)dataContext.ImageAnnotations.Count(x => x.VoteSkipped),
                TotalSubImages = (uint)dataContext.SubImageGroups.Count(x => x.ImageAnnotation != null),
                TotalBackgroundClassified = (uint)dataContext.BackgroundClassifications.Count(x => x.ImageAnnotation != null),
                TotalContextClassified = (uint)dataContext.ContextClassifications.Count(x => x.ImageAnnotation != null),
                TotalSubImaged = (uint)dataContext.SubImageGroups.Count(x => x.ImageAnnotation != null),
                TotalTrashSuperCategorised = (uint)dataContext.TrashSuperCategories.Count(x => x.SubImageAnnotation != null),
                TotalTrashSubCategorised = (uint)dataContext.TrashSubCategories.Count(x => x.SubImageAnnotation != null),
                TotalSegmentated = (uint)dataContext.Segmentations.Count(x => x.SubImageAnnotation != null),
            });
        }).Produces<DatabaseInfoDTO>();*/
    }
}