using System.Linq;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

public static class TrashSuperCategoryEndpoints
{
    public static void MapTrashSuperCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/subimageannotations/trashsupercategories/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            SubImageAnnotationEntity? nextSubImageAnnotation = null;

            foreach (var subImageAnnotation in dataContext
                .SubImageAnnotations
                .Include(x => x.SubImageAnnotationGroup)
                .ThenInclude(x => x.ImageAnnotation)
                .ThenInclude(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .AsEnumerable()
                .Where(x => x.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == x.SubImageAnnotationGroup))
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

            var subImageAnnotationDTO = new SubImageAnnotationDTO
            {
                X = nextSubImageAnnotation.X,
                Y = nextSubImageAnnotation.Y,
                Width = nextSubImageAnnotation.Width,
                Height = nextSubImageAnnotation.Height,
                SubImageAnnotationGroup = nextSubImageAnnotation.SubImageAnnotationGroup.ID,
                TrashSubCategories = nextSubImageAnnotation.TrashSubCategories.Select(x => x.ID).ToList(),
                TrashSubCategoriesConsensus = nextSubImageAnnotation.TrashSubCategoriesConsensus?.ID,
                TrashSuperCategories = nextSubImageAnnotation.TrashSuperCategories.Select(x => x.ID).ToList(),
                TrashSuperCategoriesConsensus = nextSubImageAnnotation.TrashSuperCategoriesConsensus?.ID,
                Segmentations = nextSubImageAnnotation.Segmentations.Select(x => x.ID).ToList(),
                IsComplete = nextSubImageAnnotation.IsComplete,
                IsInProgress = nextSubImageAnnotation.IsInProgress,
            };

            return Results.Ok(subImageAnnotationDTO);
        }).Produces<SubImageAnnotationDTO>();

        app.MapPost("imageannotations/subimageannotations/{id}/trashsupercategories", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, TrashSuperCategoryDTO trashSuperCategory) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.SingleOrDefault(x => x.ID == userID);
            if  (user == null)
                return Results.BadRequest("User not found");

            var subImageAnnotation = await dataContext
                .SubImageAnnotations
                .Include(x => x.TrashSubCategories)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (subImageAnnotation == null)
                return Results.NotFound("SubImageAnnotation not found");

            if (subImageAnnotation.TrashSubCategories.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            var label = trashSuperCategory.TrashSuperCategoryLabel;

            var trashSupercategoryEntity = subImageAnnotation
                .TrashSuperCategories
                .SingleOrDefault(x => x.TrashSuperCategory == label);

            if (trashSupercategoryEntity)
            {
                trashSupercategoryEntity.Users.Add(user);
            }
            else
            {
                trashSupercategoryEntity = new TrashSuperCategoryEntity
                {
                    TrashSuperCategory = label,
                    Users = new List<UserEntity> { user }
                };
                subImageAnnotation.TrashSuperCategories.Add(trashSupercategoryEntity);
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