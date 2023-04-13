using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class TrashSuperCategoryEndpoints
{
    public static void MapTrashSuperCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/trashsupercategories/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            GTTrashBoundingBoxEntity? nextTrashBoundingBox = null;

            foreach (var trashSuperCategory in dataContext
                .GTTrashBoundingBoxes
                .Where(x => !x.UserTrashSuperCategories.Any(y => y.UserId == userId)))
            {
                if (!trashSuperCategory.Consensus && trashSuperCategory.IsInProgress)
                {
                    nextTrashBoundingBox = trashSuperCategory;
                    break;
                }

                if (nextTrashBoundingBox == null && !trashSuperCategory.Consensus)
                {
                    nextTrashBoundingBox = trashSuperCategory;
                }

                nextTrashBoundingBox ??= trashSuperCategory;
            }

            if (nextTrashBoundingBox == null) return Results.NotFound();

            var trashBoudningBoxDTO = new TrashBoundingBoxDTO
            {
                Id = nextTrashBoundingBox.Id,
                TrashCountId = nextTrashBoundingBox.GTTrashCountId,
                BoundingBoxs = nextTrashBoundingBox.Data.ConvertAll(x => new Rectangle( x.X, x.Y, x.Width, x.Height)),
                Created = nextTrashBoundingBox.Created,
                Updated = nextTrashBoundingBox.Updated,
            };
            
            return Results.Ok(trashBoudningBoxDTO);
        }).Produces<TrashSuperCategoryDTO>();

        app.MapPost("/trashsupercategories/submit", async (DataContext dataContext, ClaimsPrincipal user, TrashSuperCategoryDTO trashSuperCategory) =>
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

            var trashBoundingBox = dataContext
                .GTTrashBoundingBoxes
                .Include(x => x.UserTrashSuperCategories)
                .Include(x => x.GTTrashCount)
                .ThenInclude(x => x.UserProcessings)
                .SingleOrDefault(x => x.Id == trashSuperCategory.TrashBoundingBoxId);

            if (trashBoundingBox == null)
            {
                return Results.NotFound("trashBoundingBox not found");
            }

            if (trashBoundingBox.UserTrashSuperCategories.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a TrashSuperCategory for this BoundingBox");
            }

            var userTrashSuperCategory = new UserTrashSuperCategoryEntity
            {
                UserId = userId,
                Data = trashSuperCategory.SuperCategory,
            };

            trashBoundingBox.AddUserProcessing(userTrashSuperCategory);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok();
        });
    }
}