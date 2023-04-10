using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class BackgroundClassificationEndpoints
{
    public static void MapBackgroundClassificationEndpoints(this WebApplication app)
    {
        app.MapGet("/backgroundclassification/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            ImageEntity? nextImage = null;

            foreach (var image in dataContext
                .Images
                .Where(x => !x.UserBackgroundClassifications.Any(y => y.UserId == userId)))
            {
                if (!image.Consensus && image.IsInProgress)
                {
                    nextImage = image;
                    break;
                }

                if (nextImage == null && !image.Consensus)
                {
                    nextImage = image;
                }

                nextImage ??= image;
            }

            if (nextImage == null) return Results.NotFound();

            var imageDTO = new ImageDTO
            {
                Id = nextImage.Id,
                URI = nextImage.URI,
                Created = nextImage.Created,
                Updated = nextImage.Updated,
            };

            return Results.Ok(imageDTO);
        }).Produces<ImageDTO>();

        app.MapPost("/backgroundclassification/submit", async (DataContext dataContext, ClaimsPrincipal user, BackgroundClassificationDTO backgroundClassification) =>
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

            var image = dataContext
                .Images
                .Include(x => x.UserBackgroundClassifications)
                .Include(x => x.GTBackgroundClassifications)
                .ThenInclude(x => x.UserProcessings)
                .FirstOrDefault(x => x.Id == backgroundClassification.ImageId);

            if (image == null)
            {
                return Results.NotFound("Image not found");
            }

            if (image.UserBackgroundClassifications.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");
            }

            var userBackgroundClassification = new UserBackgroundClassificationEntity
            {
                UserId = userId,
                Data = backgroundClassification.BackgroundCategory,
            };

            image.AddUserProcessing(userBackgroundClassification);

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