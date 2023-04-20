using System.Linq;
using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageAnnotationEndpoints
{
    public static void MapImageAnnotationEndpoints(this WebApplication app)
    {
        app.MapGet("imageannotations/backgroundclassifications/next", (DataContext dataContext, ClaimsPrincipal user) =>
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
                ImageID = nextImageAnnotation.Image.ID,
                BackgroundClassifications = nextImageAnnotation.BackgroundClassifications.Select(x => x.ID).ToList(),
                BackgroundClassificationConsensus = nextImageAnnotation.BackgroundClassificationConsensus?.ID,
                ContextClassifications = nextImageAnnotation.ContextClassifications.Select(x => x.ID).ToList(),
                ContextClassificationConsensus = nextImageAnnotation.ContextClassificationConsensus?.ID,
                SubImages = nextImageAnnotation.SubImages.Select(x => x.ID).ToList(),
                SubImagesConsensus = nextImageAnnotation.SubImagesConsensus.Select(x => x.ID).ToList(),
                IsInProgress = nextImageAnnotation.IsInProgress,
                IsComplete = nextImageAnnotation.IsComplete,
            };

            return Results.Ok(imageAnnotationDTO);

        }).Produces<ImageAnnotationDTO>();

        app.MapPost("imageannotations/{id}/backgroundclassifications", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, BackgroundClassificationDTO backgroundClassification) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
            {
                return Results.BadRequest("Invalid user ID format");
            }

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

            var labels = backgroundClassification.BackgroundClassificationLabels.Order().ToList();

            var backgroundclassification = imageAnnotation
                .BackgroundClassifications
                .SingleOrDefault(x => x.BackgroundClassificationStrings.Select(x => x.value).SequenceEqual(labels));

            var user = dataContext.Users.Single(x => x.ID == userID);
            if (backgroundclassification)
            {
                backgroundclassification.Users.Add(user);
            }
            else
            {
                backgroundclassification = new BackgroundClassificationEntity
                {
                    BackgroundClassificationStrings = labels.ConvertAll(x => new BackgroundClassificationStringEntity { value = x }),
                    Users = new List<UserEntity> { user }
                };
                imageAnnotation.BackgroundClassifications.Add(backgroundclassification);
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
    }
}