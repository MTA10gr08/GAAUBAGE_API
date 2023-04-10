using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class TrashBoudningBoxEndpoints
{
    public static void MapTrashBoundingBoxEndpoints(this WebApplication app)
    {
        app.MapGet("/trashboundingbox/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            GTTrashCountEntity? nextTrashCount = null;

            foreach (var trashCount in dataContext
                .GTTrashCounts
                .Where(x => !x.UserTrashBoundingBoxes.Any(y => y.UserId == userId)))
            {
                if (!trashCount.Consensus && trashCount.IsInProgress)
                {
                    nextTrashCount = trashCount;
                    break;
                }

                if (nextTrashCount == null && !trashCount.Consensus)
                {
                    nextTrashCount = trashCount;
                }

                nextTrashCount ??= trashCount;
            }

            if (nextTrashCount == null) return Results.NotFound();

            var trashCountDTO = new TrashCountDTO
            {
                Id = nextTrashCount.Id,
                ContextCassificationId = nextTrashCount.GTContextClassificationId,
                Count = nextTrashCount.Data,
                Created = nextTrashCount.Created,
                Updated = nextTrashCount.Updated,
            };

            return Results.Ok(trashCountDTO);
        }).Produces<TrashCountDTO>();

        app.MapPost("/trashboundingbox/submit", async (DataContext dataContext, ClaimsPrincipal user, TrashBoundingBoxDTO trashBoundingBox) =>
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

            var trashCount = dataContext
                .GTTrashCounts
                .Include(x => x.UserTrashBoundingBoxes)
                .ThenInclude(x => x.Data)
                .Include(x => x.GTContextClassification)
                .ThenInclude(x => x.UserProcessings)
                .Include(x => x.Data)
                .FirstOrDefault(x => x.Id == trashBoundingBox.TrashCountId);

            if (trashCount == null)
            {
                return Results.NotFound("trashCount not found");
            }

            if (trashCount.UserTrashBoundingBoxes.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a BoundingBox for this TrashCount");
            }

            var userTrashBoundingBox = new UserTrashBoundingBoxEntity
            {
                UserId = userId,
                Data = trashBoundingBox.BoundingBoxs.ConvertAll(x => new RectangleEntity
                {
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                }),
            };

            trashCount.AddUserProcessing(userTrashBoundingBox);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok();
        });
    }
}