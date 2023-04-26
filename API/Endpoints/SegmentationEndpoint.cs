using System.Linq;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

public static class SegmentationEndpoints
{
    public static void MapSegmentationEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/subimageannotations/segmentations/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            SubImageAnnotationEntity? nextSuperImageAnnotation = null;

            foreach (var subImageAnnotation in dataContext
                .SubImageAnnotations
                .Include(x => x.SubImageAnnotationGroup)
                .ThenInclude(x => x.ImageAnnotation)
                .ThenInclude(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.TrashSuperCategories)
                .ThenInclude(x => x.Users)
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Where(x => x.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == x.SubImageAnnotationGroup
                            && x.Segmentations.Any(y => y.UserID == userID)
                            && x.TrashSubCategoriesConsensus
                            && x.TrashSubCategoriesConsensus))
            {
                if (subImageAnnotation.IsInProgress)
                {
                    nextSuperImageAnnotation = subImageAnnotation;
                    break;
                }

                if (!subImageAnnotation.IsComplete)
                {
                    nextSuperImageAnnotation = subImageAnnotation;
                }

                nextSuperImageAnnotation ??= subImageAnnotation;
            }

            if (nextSuperImageAnnotation == null) return Results.NotFound();

            var subImageAnnotationDTO = new SubImageAnnotationDTO
            {
                ID = nextSuperImageAnnotation.ID,
                Created = nextSuperImageAnnotation.Created,
                Updated = nextSuperImageAnnotation.Updated,
                Image = nextSuperImageAnnotation.ImageID,
                X = nextSuperImageAnnotation.X,
                Y = nextSuperImageAnnotation.Y,
                Width = nextSuperImageAnnotation.Width,
                Height = nextSuperImageAnnotation.Height,
                SubImageAnnotationGroup = nextSuperImageAnnotation.SubImageAnnotationGroup.ID,
                TrashSubCategories = nextSuperImageAnnotation.TrashSubCategories.Select(x => x.ID).ToList(),
                TrashSubCategoriesConsensus = nextSuperImageAnnotation.TrashSubCategoriesConsensus?.ID,
                TrashSuperCategories = nextSuperImageAnnotation.TrashSuperCategories.Select(x => x.ID).ToList(),
                TrashSuperCategoriesConsensus = nextSuperImageAnnotation.TrashSuperCategoriesConsensus?.ID,
                Segmentations = nextSuperImageAnnotation.Segmentations.Select(x => x.ID).ToList(),
                IsComplete = nextSuperImageAnnotation.IsComplete,
                IsInProgress = nextSuperImageAnnotation.IsInProgress,
            };

            return Results.Ok(subImageAnnotationDTO);
        }).Produces<SubImageAnnotationDTO>();

        app.MapPost("imageannotations/segmentations/{id}/trashsubcategories", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, SegmentationDTO segmentation) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.SingleOrDefault(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var subImageAnnotation = await dataContext
                .SubImageAnnotations
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (subImageAnnotation == null)
                return Results.NotFound("SubImageAnnotation not found");

            if (subImageAnnotation.TrashSubCategories.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            subImageAnnotation.Segmentations.Add(new(){
                SubImageAnnotationID = subImageAnnotation.ID,
                UserID = userID,
                Segmentation = new(segmentation.Segmentation.Polygons.Select(x => new Polygon(new LinearRing(x.Shell.Coordinates.Select(y => new Coordinate(y.Longitude, y.Latitude)).ToArray()), x.Holes.Select(y => new LinearRing(y.Coordinates.Select(z => new Coordinate(z.Longitude, z.Latitude)).ToArray())).ToArray())).ToArray())
            });

            try
            {
                await dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = $"Failed to save changes: {ex.Message}";
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += $"\nInner exception: {innerEx.Message}";
                    innerEx = innerEx.InnerException;
                }
                return Results.BadRequest(errorMessage);
            }

            return Results.Ok();
        });
    }
}