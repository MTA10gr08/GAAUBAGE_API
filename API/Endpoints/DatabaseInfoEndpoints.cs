using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

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

                TotalSkipped = (uint)dataContext.ImageAnnotations
                .Include(x => x.VoteSkipped)
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.IsSkipped),

                TotalBackgroundClassifications = (uint)dataContext.BackgroundClassifications.Count(),

                TotalBackgroundClassified = (uint)dataContext.ImageAnnotations
                .Include(x => x.BackgroundClassifications)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.BackgroundClassificationConsensus != null),

                TotalContextClassifications = (uint)dataContext.ContextClassifications.Count(),

                TotalContextClassified = (uint)dataContext.ImageAnnotations
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.ContextClassificationConsensus != null),

                TotalSubImageGroups = (uint)dataContext.SubImageGroups.Count(),

                TotalSubImageGrouped = (uint)dataContext.ImageAnnotations
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.SubImageAnnotationGroupConsensus != null),

                TotalSubImageAnnotations = (uint)dataContext.ImageAnnotations
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.SubImageAnnotations)
                .AsEnumerable()
                .Sum(x => x.SubImageAnnotationGroups.Sum(y => y.SubImageAnnotations.Count)),

                TotalTrashSubCategorisations = (uint)dataContext.SubImageAnnotations.Count(),

                TotalTrashSubCategorised = (uint)dataContext.SubImageAnnotations
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.TrashSubCategoriesConsensus != null),

                TotalTrashSuperCategorisations = (uint)dataContext.SubImageAnnotations.Count(),

                TotalTrashSuperCategorised = (uint)dataContext.SubImageAnnotations
                .Include(x => x.TrashSuperCategories)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Count(x => x.TrashSuperCategoriesConsensus != null),

                TotalSegmentations = (uint)dataContext.Segmentations.Count(),

                TotalSegmentated = (uint)dataContext.SubImageAnnotations.Count(x => x.Segmentations.Any()),
            });
        }).Produces<DatabaseInfoDTO>();
    }
}