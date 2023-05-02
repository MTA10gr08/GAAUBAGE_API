using System.Diagnostics;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ContextClassificationEndpoints
{
    private static readonly SemaphoreSlim ContextClassificationLock = new SemaphoreSlim(1, 1);
    public static void MapContextClassificationEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/contextclassifications/next", async (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Results.BadRequest("Invalid user ID format");

            ImageAnnotationEntity? nextImageAnnotation = null;

            var imageAnnotations = await dataContext
                .ImageAnnotations
                .Where(x => !x.ContextClassifications.Any(y => y.Users.Any(w => w.ID == userId)))
                .Include(x => x.Image)
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .Include(x => x.BackgroundClassifications)
                .ThenInclude(x => x.Users)
                .ToListAsync();

            foreach (var imageAnnotation in imageAnnotations.Where(x => !x.IsSkipped))
            {
                if (imageAnnotation.IsInProgress)
                {
                    nextImageAnnotation = imageAnnotation;
                    break;
                }

                if (!imageAnnotation.IsComplete)
                {
                    nextImageAnnotation = imageAnnotation;
                }

                nextImageAnnotation ??= imageAnnotation;
            }

            if (nextImageAnnotation == null) return Results.NotFound();

            var imageAnnotationDTO = new ImageAnnotationDTO
            {
                ID = nextImageAnnotation.ID,
                Created = nextImageAnnotation.Created,
                Updated = nextImageAnnotation.Updated,
                Image = nextImageAnnotation.Image.ID,
                Skipped = nextImageAnnotation.VoteSkipped.Select(x => x.ID).ToList(),
                BackgroundClassifications = nextImageAnnotation.BackgroundClassifications.Select(x => x.ID).ToList(),
                BackgroundClassificationConsensus = nextImageAnnotation.BackgroundClassificationConsensus?.ID,
                ContextClassifications = nextImageAnnotation.ContextClassifications.Select(x => x.ID).ToList(),
                ContextClassificationConsensus = nextImageAnnotation.ContextClassificationConsensus?.ID,
                SubImages = nextImageAnnotation.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                SubImagesConsensus = nextImageAnnotation.SubImageAnnotationGroupConsensus?.ID,
                IsInProgress = nextImageAnnotation.IsInProgress,
                IsComplete = nextImageAnnotation.IsComplete,
            };

            stopwatch.Stop();
            Console.WriteLine($"CCn: {stopwatch.Elapsed}");
            return Results.Ok(imageAnnotationDTO);
        }).Produces<ImageAnnotationDTO>();

        app.MapPost("imageannotations/{id}/contextclassifications", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, ContextClassificationDTO contextClassification) =>
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

            await using (await ContextClassificationLock.WaitAsyncDisposable())
            {
                var imageAnnotation = await dataContext
                .ImageAnnotations
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == id);

                if (imageAnnotation == null)
                    return Results.NotFound("ImageAnnotation not found");

                if (imageAnnotation.BackgroundClassifications.Any(x => x.Users.Any(z => z.ID == user.ID)))
                    return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

                var label = contextClassification.ContextClassificationLabel;

                var contextClassificationEntity = imageAnnotation
                    .ContextClassifications
                    .SingleOrDefault(x => x.ContextClassification == label);

                if (contextClassificationEntity)
                {
                    contextClassificationEntity.Users.Add(user);
                }
                else
                {
                    contextClassificationEntity = new ContextClassificationEntity
                    {
                        ContextClassification = label,
                        Users = new List<UserEntity> { user }
                    };
                    imageAnnotation.ContextClassifications.Add(contextClassificationEntity);
                }

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

                stopwatch.Stop();
                Console.WriteLine($"CCp: {stopwatch.Elapsed}");
                return Results.Ok();
            }
        });
    }
}