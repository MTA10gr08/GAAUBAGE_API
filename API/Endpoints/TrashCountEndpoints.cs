using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class TrashCountEndpoints
{
    public static void MapTrashCountEndpoints(this WebApplication app)
    {
        app.MapGet("/trashcounts/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            GTContextClassificationEntity? nextContextClassification = null;

            foreach (var ContextClassification in dataContext
                .GTContextClassifications
                .Where(x => !x.UserTrashCount.Any(y => y.UserId == userId)))
            {
                if (!ContextClassification.Consensus && ContextClassification.IsInProgress)
                {
                    nextContextClassification = ContextClassification;
                    break;
                }

                if (nextContextClassification == null && !ContextClassification.Consensus)
                {
                    nextContextClassification = ContextClassification;
                }

                nextContextClassification ??= ContextClassification;
            }

            if (nextContextClassification == null) return Results.NotFound();

            var contextClassificationDTO = new ContextCassificationDTO
            {
                Id = nextContextClassification.Id,
                BackgroundClassificationId = nextContextClassification.GTBackgroundClassificationId,
                Category = nextContextClassification.Data,
                Created = nextContextClassification.Created,
                Updated = nextContextClassification.Updated,
            };

            return Results.Ok(contextClassificationDTO);
        }).Produces<ContextCassificationDTO>();

        app.MapPost("/trashcounts/submit", async (DataContext dataContext, ClaimsPrincipal user, TrashCountDTO TrashCount) =>
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

            var ContextClassification = dataContext
                .GTContextClassifications
                .Include(x => x.UserTrashCount)
                .Include(x => x.GTBackgroundClassification)
                .ThenInclude(x => x.UserProcessings)
                .SingleOrDefault(x => x.Id == TrashCount.ContextCassificationId);

            if (ContextClassification == null)
            {
                return Results.NotFound("ContextClassification not found");
            }

            if (ContextClassification.UserTrashCount.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a TrashCount for this ContextClassification");
            }

            var userTrashCount = new UserTrashCountEntity
            {
                UserId = userId,
                Data = TrashCount.Count,
            };

            ContextClassification.AddUserProcessing(userTrashCount);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok(new Guid());
        });
    }
}