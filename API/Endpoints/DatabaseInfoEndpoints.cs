using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class DatabaseInfoEndpoints
{
    public static void MapDatabaseInfoEndpoints(this WebApplication app)
    {
        app.MapGet("/databaseinfo", async (DataContext dataContext) =>
        {
            return Results.Ok(new DatabaseInfoDTO
            {
                TotalImages = (uint)dataContext.Images.Count(),

                TotalSkipped = (uint)(await dataContext.ImageAnnotations
                .Include(x => x.VoteSkipped)
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.IsSkipped),

                TotalBackgroundClassifications = (uint)await dataContext.BackgroundClassifications.CountAsync(),

                TotalBackgroundClassified = (uint)(await dataContext.ImageAnnotations
                .Include(x => x.BackgroundClassifications)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.BackgroundClassificationConsensus != null),

                TotalContextClassifications = (uint)await dataContext.ContextClassifications.CountAsync(),

                TotalContextClassified = (uint)(await dataContext.ImageAnnotations
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.ContextClassificationConsensus != null),

                TotalSubImageGroups = (uint)await dataContext.SubImageGroups.CountAsync(),

                TotalSubImageGrouped = (uint)(await dataContext.ImageAnnotations
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.SubImageAnnotationGroupConsensus != null),

                TotalSubImageAnnotations = (uint)(await dataContext.ImageAnnotations
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.SubImageAnnotations)
                .ToListAsync())
                .Sum(x => x.SubImageAnnotationGroups.Sum(y => y.SubImageAnnotations.Count)),

                TotalTrashSubCategorisations = (uint)await dataContext.SubImageAnnotations.CountAsync(),

                TotalTrashSubCategorised = (uint)(await dataContext.SubImageAnnotations
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.TrashSubCategoriesConsensus != null),

                TotalTrashSuperCategorisations = (uint)await dataContext.SubImageAnnotations.CountAsync(),

                TotalTrashSuperCategorised = (uint)(await dataContext.SubImageAnnotations
                .Include(x => x.TrashSuperCategories)
                .ThenInclude(x => x.Users)
                .ToListAsync())
                .Count(x => x.TrashSuperCategoriesConsensus != null),

                TotalSegmentations = (uint)await dataContext.Segmentations.CountAsync(),

                TotalSegmentated = (uint)await dataContext.SubImageAnnotations.CountAsync(x => x.Segmentations.Any()),
            });
        }).Produces<DatabaseInfoDTO>();
    }
}