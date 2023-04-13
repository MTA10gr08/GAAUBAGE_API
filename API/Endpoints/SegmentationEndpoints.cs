using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace API.Endpoints;
public static class SegmentationEndpoints
{
    public static void MapSegmentationEndpoints(this WebApplication app)
    {
        app.MapGet("/segmentations/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Results.BadRequest("Invalid user ID format");
            }

            GTTrashCategoryEntity? nextCategory = null;

            foreach (var TrashCategory in dataContext
                .GTTrashCategories
                .Where(x => !x.UserSegmentations.Any(y => y.UserId == userId)))
            {
                if (!TrashCategory.Consensus && TrashCategory.IsInProgress)
                {
                    nextCategory = TrashCategory;
                    break;
                }

                if (nextCategory == null && !TrashCategory.Consensus)
                {
                    nextCategory = TrashCategory;
                }

                nextCategory ??= TrashCategory;
            }

            if (nextCategory == null) return Results.NotFound();

            var trashCategoryDTO = new TrashCategoryDTO
            {
                Id = nextCategory.Id,
                TrashSuperCategoryId = nextCategory.GTTrashSuperCategoryId,
                Category = nextCategory.Data,
                Created = nextCategory.Created,
                Updated = nextCategory.Updated,
            };

            return Results.Ok(trashCategoryDTO);
        }).Produces<TrashCategoryDTO>();

        app.MapPost("/segmentations/submit", async (DataContext dataContext, ClaimsPrincipal user, SegmentationDTO segmentation) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Results.BadRequest("Invalid user ID format");
            }

            var trashCategory = dataContext
                .GTTrashCategories
                .Include(e => e.UserSegmentations)
                .Include(e => e.GTSegmentations)
                .ThenInclude(e => e.UserProcessings)
                .FirstOrDefault(x => x.Id == segmentation.TrashBoundingBoxId);

            if (trashCategory == null)
            {
                return Results.BadRequest("Invalid image ID");
            }

            if (trashCategory.UserSegmentations.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a Segmentation for this TrashCategory");
            }

            var userSegmentation = new UserSegmentationEntity
            {
                UserId = userId,
                Data = new(segmentation.Segmentation.Polygons.Select(x => new Polygon(new LinearRing(x.Shell.Coordinates.Select(y => new Coordinate(y.Longitude, y.Latitude)).ToArray()), x.Holes.Select(y => new LinearRing(y.Coordinates.Select(z => new Coordinate(z.Longitude, z.Latitude)).ToArray())).ToArray())).ToArray()),
            };

            trashCategory.AddUserProcessing(userSegmentation);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok();
        });
    }
}