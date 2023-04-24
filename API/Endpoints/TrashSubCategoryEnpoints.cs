using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
/*
public static class TrashSubCategories
{

    public static void MapContextClassificationEndpoints(this WebApplication app)
    {
        app.MapGet("imageannotations/trashsubcategories/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            SubImageAnnotationEntity? nextSubImageAnnotation = null;

            foreach (var subImageAnnotation in dataContext
                .ImageAnnotations
                .Where(x => x.SubImageAnnotationGroupConsensus != null)
                .Where(x => !x.SubImageAnnotationGroupConsensus.SubImageAnnotations.Any(y => y.TrashSubCategories.Any(w => w.Users.Any(z => z.ID == userID))))
                .Select(x => x.SubImageAnnotationGroupConsensus.SubImageAnnotations))
            {
                if (subImageAnnotation.IsInProgress)
                {
                    nextSubImageAnnotation = subImageAnnotation;
                    break;
                }

                if (!subImageAnnotation.IsComplete)
                {
                    nextSubImageAnnotation = subImageAnnotation;
                }

                nextSubImageAnnotation ??= subImageAnnotation;
            }

            if (nextSubImageAnnotation == null) return Results.NotFound();

            var imageAnnotationDTO = new ImageAnnotationDTO
            {
                ID = nextSubImageAnnotation.ID,
                ImageID = nextSubImageAnnotation.Image.ID,
                BackgroundClassifications = nextSubImageAnnotation.BackgroundClassifications.Select(x => x.ID).ToList(),
                BackgroundClassificationConsensus = nextSubImageAnnotation.BackgroundClassificationConsensus?.ID,
                ContextClassifications = nextSubImageAnnotation.ContextClassifications.Select(x => x.ID).ToList(),
                ContextClassificationConsensus = nextSubImageAnnotation.ContextClassificationConsensus?.ID,
                SubImages = nextSubImageAnnotation.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                SubImagesConsensus = nextSubImageAnnotation.SubImageAnnotationGroupConsensus?.ID,
                IsInProgress = nextSubImageAnnotation.IsInProgress,
                IsComplete = nextSubImageAnnotation.IsComplete,
            };

            return Results.Ok(imageAnnotationDTO);
        }).Produces<ImageAnnotationDTO>();

        app.MapPost("imageannotations/{id}/trashsubcategories", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, ContextClassificationDTO contextClassification) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var imageAnnotation = await dataContext
                .ImageAnnotations
                .Include(x => x.ContextClassifications)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (imageAnnotation == null)
                return Results.NotFound("ImageAnnotation not found");

            if (imageAnnotation.BackgroundClassifications.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            var label = contextClassification.ContextClassificationLabel;

            var contextClassificationEntity = imageAnnotation
                .ContextClassifications
                .SingleOrDefault(x => x.ContextClassification == label);

            var user = dataContext.Users.Single(x => x.ID == userID);
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

            return Results.Ok();
        });
    }
}*/