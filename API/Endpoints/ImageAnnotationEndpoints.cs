using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageAnnotationEndpoints
{
    public static void MapImageAnnotationEndpoints(this WebApplication app)
    {
        app.MapPost("/imageannotations/{id}/voteskip", async (Guid id, DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = await dataContext.Users.SingleOrDefaultAsync(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var imageAnnotation = await dataContext.ImageAnnotations.FirstOrDefaultAsync(x => x.ID == id);

            if (imageAnnotation == null)
                return Results.NotFound("ImageAnnotation not found");

            if (imageAnnotation.VoteSkipped.Any(x => x.ID == user.ID))
                return Results.BadRequest("User has already vote skipped this annotation");

            imageAnnotation.VoteSkipped.Add(user);

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
        }).Produces<ImageAnnotationDTO>().RequireAuthorization();

        app.MapGet("/imageannotations/{id}", async (Guid id, DataContext dataContext) =>
        {
            var imageAnnotationEntity = await dataContext.ImageAnnotations.FirstOrDefaultAsync(x => x.ID == id);
            if (imageAnnotationEntity != null)
            {
                var imageAnnotationDTO = new ImageAnnotationDTO
                {
                    ID = imageAnnotationEntity.ID,
                    Created = imageAnnotationEntity.Created,
                    Updated = imageAnnotationEntity.Updated,
                    Image = imageAnnotationEntity.ImageID,
                    Skipped = imageAnnotationEntity.VoteSkipped.Select(x => x.ID).ToList(),
                    BackgroundClassifications = imageAnnotationEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
                    BackgroundClassificationConsensus = imageAnnotationEntity.BackgroundClassificationConsensus?.ID,
                    ContextClassifications = imageAnnotationEntity.ContextClassifications.Select(x => x.ID).ToList(),
                    ContextClassificationConsensus = imageAnnotationEntity.ContextClassificationConsensus?.ID,
                    SubImages = imageAnnotationEntity.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                    SubImagesConsensus = imageAnnotationEntity.SubImageAnnotationGroupConsensus?.ID,
                    IsInProgress = imageAnnotationEntity.IsInProgress,
                    IsComplete = imageAnnotationEntity.IsComplete,
                };
                return Results.Ok(imageAnnotationDTO);
            }
            return Results.BadRequest();
        }).Produces<ImageAnnotationDTO>().RequireAuthorization();
    }
}