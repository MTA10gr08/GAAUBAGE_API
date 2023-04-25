using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class BackgroundclassifiCationEndpoints
{
    public static void MapBackgroundclassifiCationEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/backgroundclassifications/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Results.BadRequest("Invalid user ID format");

            ImageAnnotationEntity? nextImageAnnotation = null;

            foreach (var imageAnnotation in dataContext
                .ImageAnnotations
                .Where(x => !x.BackgroundClassifications.Any(y => y.Users.Any(w => w.ID == userId)))
                .Include(x => x.Image))
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
                BackgroundClassifications = nextImageAnnotation.BackgroundClassifications.Select(x => x.ID).ToList(),
                BackgroundClassificationConsensus = nextImageAnnotation.BackgroundClassificationConsensus?.ID,
                ContextClassifications = nextImageAnnotation.ContextClassifications.Select(x => x.ID).ToList(),
                ContextClassificationConsensus = nextImageAnnotation.ContextClassificationConsensus?.ID,
                SubImages = nextImageAnnotation.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                SubImagesConsensus = nextImageAnnotation.SubImageAnnotationGroupConsensus?.ID,
                IsInProgress = nextImageAnnotation.IsInProgress,
                IsComplete = nextImageAnnotation.IsComplete,
            };

            return Results.Ok(imageAnnotationDTO);

        }).Produces<ImageAnnotationDTO>();

        app.MapPost("imageannotations/{id}/backgroundclassifications", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, BackgroundClassificationDTO backgroundClassification) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.SingleOrDefault(x => x.ID == userID);
            if  (user == null)
                return Results.BadRequest("User not found");

            var imageAnnotation = dataContext
                .ImageAnnotations
                .Include(x => x.BackgroundClassifications)
                .ThenInclude(x => x.Users)
                .Include(x => x.BackgroundClassifications)
                .ThenInclude(x => x.BackgroundClassificationStrings)
                .FirstOrDefault(x => x.ID == id);

            if (imageAnnotation == null)
                return Results.NotFound("ImageAnnotation not found");

            if (imageAnnotation.BackgroundClassifications.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            var labels = backgroundClassification.BackgroundClassificationLabels.OrderBy(x => x).ToList();

            var backgroundclassificationEntitiy = imageAnnotation
                .BackgroundClassifications
                .SingleOrDefault(x => x.BackgroundClassificationStrings.Select(x => x.value).SequenceEqual(labels));

            if (backgroundclassificationEntitiy)
            {
                backgroundclassificationEntitiy.Users.Add(user);
            }
            else
            {
                backgroundclassificationEntitiy = new BackgroundClassificationEntity
                {
                    BackgroundClassificationStrings = labels.ConvertAll(x => new BackgroundClassificationStringEntity { value = x }),
                    Users = new List<UserEntity> { user }
                };
                imageAnnotation.BackgroundClassifications.Add(backgroundclassificationEntitiy);
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

            return Results.Ok();
        });

        app.MapGet("imageannotations/backgroundclassifications", (DataContext dataContext) =>
        {
            var imageAnnotation = dataContext.BackgroundClassifications;

            var backgroundClassifications = imageAnnotation.Select(x => new BackgroundClassificationDTO
            {
                ID = x.ID,
                Created = x.Created,
                Updated = x.Updated,
                BackgroundClassificationLabels = x.BackgroundClassificationStrings.Select(x => x.value).ToList(),
                Users = x.Users.Select(x => x.ID).ToList(),
                ImageAnnotation = x.ImageAnnotation.ID,
            }).ToList();

            return Results.Ok();
        });
    }
}