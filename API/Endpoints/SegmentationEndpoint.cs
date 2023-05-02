using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace API.Endpoints;
public static class SegmentationEndpoints
{
    private static readonly SemaphoreSlim SegmentationLock = new SemaphoreSlim(1, 1);
    public static void MapSegmentationEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/subimageannotations/segmentations/next", async (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = await dataContext.Users.SingleOrDefaultAsync(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            SubImageAnnotationEntity? nextSubImageAnnotation = null;


            var dbstopwatch = new Stopwatch();
            dbstopwatch.Start();
            var subImageAnnotations = await dataContext
                .SubImageAnnotations
                .Include(x => x.SubImageAnnotationGroup)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroup)
                .ThenInclude(x => x.ImageAnnotation)
                .ThenInclude(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.TrashSuperCategories)
                .ThenInclude(x => x.Users)
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .Include(x => x.Segmentations)
                .AsSplitQuery()
                .ToListAsync();
            dbstopwatch.Stop();
            Console.WriteLine($"SenDB: {dbstopwatch.Elapsed}");

            var sestopwatch = new Stopwatch();
            sestopwatch.Start();
            int priority = 0;
            foreach (var subImageAnnotation in subImageAnnotations
                .Where(x => (x.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == x.SubImageAnnotationGroup
                            || x.SubImageAnnotationGroup.Users.Any(y => y.ID == userID))
                            && !x.Segmentations.Any(y => y.UserID == user.ID)
                            && x.TrashSubCategoriesConsensus
                            && x.TrashSuperCategoriesConsensus))
            {
                bool consensus = subImageAnnotation.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == subImageAnnotation.SubImageAnnotationGroup;
                bool userInGroup = subImageAnnotation.SubImageAnnotationGroup.Users.Any(y => y.ID == userID);

                if (consensus && subImageAnnotation.IsInProgress)
                {
                    priority = 6;
                    nextSubImageAnnotation = subImageAnnotation;
                    break;
                }
                else if (consensus && !subImageAnnotation.IsInProgress && priority < 5)
                {
                    priority = 5;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (consensus && subImageAnnotation.IsComplete && priority < 4)
                {
                    priority = 4;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (userInGroup && subImageAnnotation.IsInProgress && priority < 3)
                {
                    priority = 3;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (userInGroup && !subImageAnnotation.IsInProgress && priority < 2)
                {
                    priority = 2;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (!nextSubImageAnnotation && priority < 1)
                {
                    priority = 1;
                    nextSubImageAnnotation = subImageAnnotation;
                }
            }
            sestopwatch.Stop();
            Console.WriteLine($"SenSE: {sestopwatch.Elapsed}");

            if (nextSubImageAnnotation == null) return Results.NotFound();

            var subImageAnnotationDTO = new SubImageAnnotationDTO
            {
                ID = nextSubImageAnnotation.ID,
                Created = nextSubImageAnnotation.Created,
                Updated = nextSubImageAnnotation.Updated,
                Image = nextSubImageAnnotation.ImageID,
                X = nextSubImageAnnotation.X,
                Y = nextSubImageAnnotation.Y,
                Width = nextSubImageAnnotation.Width,
                Height = nextSubImageAnnotation.Height,
                SubImageAnnotationGroup = nextSubImageAnnotation.SubImageAnnotationGroup.ID,
                TrashSubCategories = nextSubImageAnnotation.TrashSubCategories.Select(x => x.ID).ToList(),
                TrashSubCategoriesConsensus = nextSubImageAnnotation.TrashSubCategoriesConsensus?.ID,
                TrashSuperCategories = nextSubImageAnnotation.TrashSuperCategories.Select(x => x.ID).ToList(),
                TrashSuperCategoriesConsensus = nextSubImageAnnotation.TrashSuperCategoriesConsensus?.ID,
                Segmentations = nextSubImageAnnotation.Segmentations.Select(x => x.ID).ToList(),
                IsComplete = nextSubImageAnnotation.IsComplete,
                IsInProgress = nextSubImageAnnotation.IsInProgress,
            };

            stopwatch.Stop();
            Console.WriteLine($"Sen: {stopwatch.Elapsed}");
            return Results.Ok(subImageAnnotationDTO);
        }).Produces<SubImageAnnotationDTO>();

        app.MapPost("imageannotations/subimageannotations/{id}/segmentations", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, SegmentationDTO segmentation) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = await dataContext.Users.SingleOrDefaultAsync(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            await SegmentationLock.WaitAsync();

            var subImageAnnotation = await dataContext
                .SubImageAnnotations
                .Include(x => x.Segmentations)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (subImageAnnotation == null)
                return Results.NotFound("SubImageAnnotation not found");

            if (subImageAnnotation.TrashSubCategories.Any(x => x.Users.Any(z => z.ID == user.ID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            subImageAnnotation.Segmentations.Add(new()
            {
                SubImageAnnotationID = subImageAnnotation.ID,
                UserID = userID,
                Segmentation = GeometryConverter.ToMultiPolygon(segmentation.SegmentationMultiPolygon)
                //Segmentation = new(segmentation.Segmentation.Polygons.Select(x => new Polygon(new LinearRing(x.Shell.Coordinates.Select(y => new Coordinate(y.Longitude, y.Latitude)).ToArray()), x.Holes.Select(y => new LinearRing(y.Coordinates.Select(z => new Coordinate(z.Longitude, z.Latitude)).ToArray())).ToArray())).ToArray())
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
            finally
            {
                SegmentationLock.Release();
            }

            stopwatch.Stop();
            Console.WriteLine($"Sep: {stopwatch.Elapsed}");
            return Results.Ok();
        });
    }
}