using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class TrashCategoryEndpoints
{
    public static void MapTrashCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/trashcategory/next", (DataContext dataContext, ClaimsPrincipal user) =>
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

            GTTrashSuperCategoryEntity? nextSuperCategory = null;

            foreach (var SuperCategory in dataContext
                .GTTrashSuperCategories
                .Where(x => !x.UserTrashCategory.Any(y => y.UserId == userId)))
            {
                if (!SuperCategory.Consensus && SuperCategory.IsInProgress)
                {
                    nextSuperCategory = SuperCategory;
                    break;
                }

                if (nextSuperCategory == null && !SuperCategory.Consensus)
                {
                    nextSuperCategory = SuperCategory;
                }

                nextSuperCategory ??= SuperCategory;
            }

            if (nextSuperCategory == null) return Results.NotFound();

            var trashSuperCategoryDTO = new TrashSuperCategoryDTO
            {
                Id = nextSuperCategory.Id,
                TrashBoundingBoxId = nextSuperCategory.GTTrashBoundingBoxId,
                SuperCategory = nextSuperCategory.Data,
                Created = nextSuperCategory.Created,
                Updated = nextSuperCategory.Updated,
            };

            return Results.Ok(trashSuperCategoryDTO);
        }).Produces<TrashSuperCategoryDTO>();

        app.MapPost("/trashcategory/submit", async (DataContext dataContext, ClaimsPrincipal user, TrashCategoryDTO trashCategory) =>
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

            var trashSuperCategory = dataContext
                .GTTrashSuperCategories
                .Include(x => x.UserTrashCategory)
                .Include(x => x.GTTrashBoundingBox)
                .ThenInclude(x => x.UserProcessings)
                .FirstOrDefault(x => x.Id == trashCategory.TrashSuperCategoryId);

            if (trashSuperCategory == null)
            {
                return Results.BadRequest("Invalid SuperCategory ID");
            }

            if (trashSuperCategory.UserTrashCategory.Any(x => x.UserId == userId))
            {
                return Results.BadRequest("User has already submitted a TrashCategory for this TrashSuperCategory");
            }

            var userTrashClassification = new UserTrashCategoryEntity
            {
                UserId = userId,
                Data = trashCategory.Category,
            };

            trashSuperCategory.AddUserProcessing(userTrashClassification);

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok();
        });
    }
}