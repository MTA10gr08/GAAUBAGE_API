using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class TrashSuperCategoryEndpoints
{
    private static readonly SemaphoreSlim trashSuperCategoryLock = new SemaphoreSlim(1, 1);
    public static void MapTrashSuperCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/subimageannotations/trashsupercategories/next", async (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            SubImageAnnotationEntity? nextSubImageAnnotation = null;

            var subImageAnnotations = await dataContext
                .SubImageAnnotations
                .Include(x => x.SubImageAnnotationGroup)
                .ThenInclude(x => x.ImageAnnotation)
                .ThenInclude(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.TrashSuperCategories)
                .ThenInclude(x => x.Users)
                .AsSplitQuery()
                .ToListAsync();

            int priority = 0;
            foreach (var subImageAnnotation in subImageAnnotations
                .Where(x => (x.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == x.SubImageAnnotationGroup
                            || x.SubImageAnnotationGroup.Users.Any(y => y.ID == userID))
                            && !x.TrashSuperCategories.Any(y => y.Users.Any(z => z.ID == userID))))
            {
                bool consensus = subImageAnnotation.SubImageAnnotationGroup.ImageAnnotation.SubImageAnnotationGroupConsensus == subImageAnnotation.SubImageAnnotationGroup;
                bool userInGroup = subImageAnnotation.SubImageAnnotationGroup.Users.Any(y => y.ID == userID);

                if (consensus && subImageAnnotation.IsInProgress)
                {
                    priority = 6;
                    nextSubImageAnnotation = subImageAnnotation;
                    break;
                }
                else if (consensus && !subImageAnnotation.IsInProgress && priority < 5)
                {
                    priority = 5;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (consensus && subImageAnnotation.IsComplete && priority < 4)
                {
                    priority = 4;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (userInGroup && subImageAnnotation.IsInProgress && priority < 3)
                {
                    priority = 3;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (userInGroup && !subImageAnnotation.IsInProgress && priority < 2)
                {
                    priority = 2;
                    nextSubImageAnnotation = subImageAnnotation;
                }
                else if (!nextSubImageAnnotation && priority < 1)
                {
                    priority = 1;
                    nextSubImageAnnotation = subImageAnnotation;
                }
            }

            if (nextSubImageAnnotation == null) return Results.NotFound();

            var subImageAnnotationDTO = new SubImageAnnotationDTO
            {
                ID = nextSubImageAnnotation.ID,
                Created = nextSubImageAnnotation.Created,
                Updated = nextSubImageAnnotation.Updated,
                Image = nextSubImageAnnotation.ImageID,
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
            if (user == null)
                return Results.BadRequest("User not found");

            await using (await trashSuperCategoryLock.WaitAsyncDisposable())
            {

                var subImageAnnotation = await dataContext
                    .SubImageAnnotations
                    .Include(x => x.TrashSuperCategories)
                    .ThenInclude(x => x.Users)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(x => x.ID == id);

                if (subImageAnnotation == null)
                    return Results.NotFound("SubImageAnnotation not found");

                if (subImageAnnotation.TrashSuperCategories.Any(x => x.Users.Any(z => z.ID == user.ID)))
                    return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

                var label = trashSuperCategory.TrashSuperCategoryLabel;

                var trashSupercategoryEntity = subImageAnnotation
                    .TrashSuperCategories
                    .SingleOrDefault(x => x.TrashSuperCategory == label);

                if (trashSupercategoryEntity)
                {
                    trashSupercategoryEntity?.Users.Add(user);
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
            }
        });
    }
}