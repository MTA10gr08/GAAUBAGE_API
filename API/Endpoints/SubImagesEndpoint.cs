using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Hungarian = HungarianAlgorithm.HungarianAlgorithm;

namespace API.Endpoints;
public static class SubImageEndpoints
{
    public static void MapSubImageEndpoints(this WebApplication app)
    {
        app.MapGet("/imageannotations/subimages/next", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Results.BadRequest("Invalid user ID format");

            ImageAnnotationEntity? nextImageAnnotation = null;

            foreach (var imageAnnotation in dataContext
                .ImageAnnotations
                .Where(x => !x.SubImageAnnotationGroups.Any(y => y.Users.Any(z => z.ID == userId)))
                .Include(x => x.Image))
            {
                if (imageAnnotation.IsInProgress)
                {
                    nextImageAnnotation = imageAnnotation;
                    break;
                }

                if (!imageAnnotation.IsComplete) // should not be if annotation is complete but if it has a sub image that is not complete
                {
                    nextImageAnnotation = imageAnnotation;
                }

                nextImageAnnotation ??= imageAnnotation;
            }

            if (nextImageAnnotation == null) return Results.NotFound();

            var imageAnnotationDTO = new ImageAnnotationDTO
            {
                ID = nextImageAnnotation.ID,
                Created = nextImageAnnotation.Created,
                Updated = nextImageAnnotation.Updated,
                Image = nextImageAnnotation.Image.ID,
                BackgroundClassifications = nextImageAnnotation.BackgroundClassifications.Select(x => x.ID).ToList(),
                BackgroundClassificationConsensus = nextImageAnnotation.BackgroundClassificationConsensus?.ID,
                ContextClassifications = nextImageAnnotation.ContextClassifications.Select(x => x.ID).ToList(),
                ContextClassificationConsensus = nextImageAnnotation.ContextClassificationConsensus?.ID,
                SubImages = nextImageAnnotation.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                SubImagesConsensus = nextImageAnnotation.SubImageAnnotationGroupConsensus?.ID,
                IsInProgress = nextImageAnnotation.IsInProgress,
                IsComplete = nextImageAnnotation.IsComplete,
            };

            return Results.Ok(imageAnnotationDTO);
        }).Produces<ImageAnnotationDTO>();


        app.MapGet("imageannotations/subimages", (DataContext dataContext) =>
        {
            var subImageAnnotations = dataContext.SubImageAnnotations.Select(x => new SubImageAnnotationDTO
            {
                ID = x.ID,
                Created = x.Created,
                Updated = x.Updated,
                X = x.X,
                Y = x.Y,
                Width = x.Width,
                Height = x.Height,
                SubImageAnnotationGroup = x.SubImageAnnotationGroupID,
                TrashSuperCategories = x.TrashSuperCategories.Select(y => y.ID).ToList(),
                TrashSuperCategoriesConsensus = x.TrashSuperCategoriesConsensus.ID,
                TrashSubCategories = x.TrashSubCategories.Select(y => y.ID).ToList(),
                TrashSubCategoriesConsensus = x.TrashSubCategoriesConsensus.ID,
                Segmentations = x.Segmentations.Select(y => y.ID).ToList(),
                IsComplete = x.IsComplete,
                IsInProgress = x.IsInProgress,
            }).ToList();
            return Results.Ok(subImageAnnotations);
        }).Produces<List<SubImageAnnotationDTO>>();

        app.MapGet("imageannotations/subimagegroups", (DataContext dataContext) =>
        {
            var subImageAnnotations = dataContext.SubImageGroups.Select(x => new SubImageAnnotationGroupDTO
            {
                Users = x.Users.Select(y => y.ID).ToList(),
                SubImageAnnotations = x.SubImageAnnotations.Select(y => new SubImageAnnotationDTO
                {
                    ID = y.ID,
                    Created = y.Created,
                    Updated = y.Updated,
                    X = y.X,
                    Y = y.Y,
                    Width = y.Width,
                    Height = y.Height,
                    Image = y.ImageID,
                    SubImageAnnotationGroup = y.SubImageAnnotationGroupID,
                    TrashSuperCategories = y.TrashSuperCategories.Select(z => z.ID).ToList(),
                    TrashSuperCategoriesConsensus = y.TrashSuperCategoriesConsensus.ID,
                    TrashSubCategories = y.TrashSubCategories.Select(z => z.ID).ToList(),
                    TrashSubCategoriesConsensus = y.TrashSubCategoriesConsensus.ID,
                    Segmentations = y.Segmentations.Select(z => z.ID).ToList(),
                    IsComplete = y.IsComplete,
                    IsInProgress = y.IsInProgress,
                }).ToList(),
                ImageAnnotation = x.ImageAnnotationID,
            }).ToList();
            return Results.Ok(subImageAnnotations);
        }).Produces<List<SubImageAnnotationGroupDTO>>();


        app.MapPost("imageannotations/{id}/subimages", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, SubImageAnnotationGroupDTO subImageAnnotation) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.SingleOrDefault(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var imageAnnotation = await dataContext
                .ImageAnnotations
                .Include(x => x.Image)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.SubImageAnnotations)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (imageAnnotation == null)
                return Results.NotFound("ImageAnnotation not found");

            if (imageAnnotation.SubImageAnnotationGroups.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a subimages for this image");

            /*if (imageAnnotation.ImageID == Guid.Empty)
                return Results.BadRequest("ImageAnnotation has no image: " + imageAnnotation.Image.ID);*/

            FindAndUpdateBestFitGroup(ref imageAnnotation, ref subImageAnnotation, ref user, 0.5f);

            /*imageAnnotation.SubImageAnnotationGroups.Add(new SubImageAnnotationGroupEntity{
                ImageAnnotationID = imageAnnotation.ID,
                Users = new List<UserEntity>{user},
                SubImageAnnotations = subImageAnnotation.SubImages.Select(x => new SubImageAnnotationEntity{X = x.X, Y = x.Y, Width = x.Width, Height = x.Height}).ToList(),
            });*/

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

    public static double CalculateIOU(SubImageAnnotationDTO boxA, SubImageAnnotationDTO boxB)
    {
        uint x1 = Math.Max(boxA.X, boxB.X);
        uint y1 = Math.Max(boxA.Y, boxB.Y);
        uint x2 = Math.Min(boxA.X + boxA.Width, boxB.X + boxB.Width);
        uint y2 = Math.Min(boxA.Y + boxA.Height, boxB.Y + boxB.Height);

        uint intersectionWidth = x2 > x1 ? x2 - x1 : 0;
        uint intersectionHeight = y2 > y1 ? y2 - y1 : 0;
        uint intersectionArea = intersectionWidth * intersectionHeight;

        uint boxAArea = boxA.Width * boxA.Height;
        uint boxBArea = boxB.Width * boxB.Height;
        uint unionArea = boxAArea + boxBArea - intersectionArea;

        return (double)intersectionArea / unionArea;
    }

    private static void FindAndUpdateBestFitGroup(ref ImageAnnotationEntity imageAnnotation, ref SubImageAnnotationGroupDTO subImageAnnotation, ref UserEntity user, float iouThreshold)
    {
        var count = subImageAnnotation.SubImageAnnotations.Count;

        var subImageAnnotationGroups = imageAnnotation
                .SubImageAnnotationGroups
                .Where(x => x.SubImageAnnotations.Count == count).ToList();

        SubImageAnnotationGroupEntity bestFitGroup = null;
        double maxMinIoU = -1;
        int[] bestAssignment = null;

        foreach (var group in subImageAnnotationGroups)
        {
            // Convert SubImageAnnotationGroupDTO to a list of SubImageAnnotationDTO objects
            List<SubImageAnnotationDTO> newBoundingBoxes = subImageAnnotation.SubImageAnnotations.ToList();

            // Calculate the cost matrix
            int n = group.SubImageAnnotations.Count;
            int m = newBoundingBoxes.Count;
            double[,] doubleCostMatrix = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    SubImageAnnotationDTO oldBoxDTO = new() { X = group.SubImageAnnotations.ElementAt(i).X, Y = group.SubImageAnnotations.ElementAt(i).Y, Width = group.SubImageAnnotations.ElementAt(i).Width, Height = group.SubImageAnnotations.ElementAt(i).Height };
                    double iou = CalculateIOU(oldBoxDTO, newBoundingBoxes[j]);
                    doubleCostMatrix[i, j] = 1 - iou; // Cost is 1 - IOU
                }
            }

            int scalingFactor = 1000;
            int[,] costMatrix = new int[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    costMatrix[i, j] = (int)Math.Round(doubleCostMatrix[i, j] * scalingFactor);
                }
            }

            // Find the optimal assignment
            int[] assignment = Hungarian.FindAssignments(costMatrix);

            // Find the minimum IOU among the assigned pairs
            double minIoU = double.MaxValue;
            for (int i = 0; i < n; i++)
            {
                double iou = 1 - costMatrix[i, assignment[i]]; // Convert cost back to IOU
                if (iou < minIoU)
                {
                    minIoU = iou;
                }
            }

            // Check if the minimum IOU is better than the current best fit
            if (minIoU > maxMinIoU && minIoU >= iouThreshold)
            {
                maxMinIoU = minIoU;
                bestFitGroup = group;
                bestAssignment = assignment;
            }
        }

        if (bestFitGroup != null)
        {
            // Update the average bounding boxes for the best fit group
            int newUserCount = bestFitGroup.Users.Count + 1;
            for (int i = 0; i < bestFitGroup.SubImageAnnotations.Count; i++)
            {
                var oldBox = bestFitGroup.SubImageAnnotations.ElementAt(i);
                var newBox = subImageAnnotation.SubImageAnnotations.ToList()[bestAssignment[i]];

                oldBox.X = (uint)((oldBox.X * (newUserCount - 1) + newBox.X) / newUserCount);
                oldBox.Y = (uint)((oldBox.Y * (newUserCount - 1) + newBox.Y) / newUserCount);
                oldBox.Width = (uint)((oldBox.Width * (newUserCount - 1) + newBox.Width) / newUserCount);
                oldBox.Height = (uint)((oldBox.Height * (newUserCount - 1) + newBox.Height) / newUserCount);
            }

            // Update the Users collection if necessary
            if (!bestFitGroup.Users.Contains(user))
            {
                bestFitGroup.Users.Add(user);
            }
        }
        else
        {
            var imageID = imageAnnotation.Image.ID;
            var subImageAnnotationGroupToAdd = new SubImageAnnotationGroupEntity()
            {
                ImageAnnotationID = imageAnnotation.ID,
                Users = new List<UserEntity> { user },
                SubImageAnnotations = subImageAnnotation.SubImageAnnotations.Select(x => new SubImageAnnotationEntity
                {
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height,
                    ImageID = imageID
                }).ToList(),
            };
            imageAnnotation.SubImageAnnotationGroups.Add(subImageAnnotationGroupToAdd);
        }
    }
}