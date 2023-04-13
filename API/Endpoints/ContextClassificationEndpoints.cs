using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ContextClassificationEndpoints
{
    public static void MapContextClassificationEndpoints(this WebApplication app)
    {
        app.MapGet("/contextclassifications/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            GTBackgroundClassificationEntity? nextBackgroundClassification = null;

            foreach (var BackgroundClassification in dataContext
                .GTBackgroundClassifications
                .Where(x => !x.UserContextClassifications.Any(y => y.UserId == userId)))
            {
                if (!BackgroundClassification.Consensus && BackgroundClassification.IsInProgress)
                {
                    nextBackgroundClassification = BackgroundClassification;
                    break;
                }

                if (nextBackgroundClassification == null && !BackgroundClassification.Consensus)
                {
                    nextBackgroundClassification = BackgroundClassification;
                }

                nextBackgroundClassification ??= BackgroundClassification;
            }

            if (nextBackgroundClassification == null) return Results.NotFound();

            var BackgroundClassificationDTO = new BackgroundClassificationDTO
            {
                Id = nextBackgroundClassification.Id,
                ImageId = nextBackgroundClassification.ImageId,
                BackgroundCategory = nextBackgroundClassification.Data,
                Created = nextBackgroundClassification.Created,
                Updated = nextBackgroundClassification.Updated,
            };

            return Results.Ok(BackgroundClassificationDTO);
        }).Produces<BackgroundClassificationDTO>();

        app.MapPost("/contextclassifications/submit", async (DataContext dataContext, ClaimsPrincipal user, ContextCassificationDTO contextClassification) =>
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

            var BackgroundClassification = dataContext
                .GTBackgroundClassifications
                .Include(x => x.UserContextClassifications)
                .Include(x => x.GTContextClassifications)
                .ThenInclude(x => x.UserProcessings)
                .FirstOrDefault(x => x.Id == contextClassification.BackgroundClassificationId);

            if (BackgroundClassification == null)
            {
                return Results.NotFound("BackgroundClassification not found");
            }

            if (BackgroundClassification.UserContextClassifications.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a ContextClassification for this BackgroundClassification");
            }

            var userContextClassification = new UserContextClassificationEntity
            {
                UserId = userId,
                Data = contextClassification.Category,
            };

            BackgroundClassification.AddUserProcessing(userContextClassification);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok();
        });
    }
}