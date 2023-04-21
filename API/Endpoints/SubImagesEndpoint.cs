using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Hungarian = HungarianAlgorithm.HungarianAlgorithm;

namespace API.Endpoints;
public static class SubImageEndpoints
{
    public static void MapSubImageEndpoints(this WebApplication app)
    {
        app.MapGet("imageannotations/SubImageEndpoints/next", (DataContext dataContext, ClaimsPrincipal user) =>
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
                ImageID = nextImageAnnotation.Image.ID,
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

        app.MapPost("imageannotations/{id}/SubImageEndpoints", async (Guid id, DataContext dataContext, ClaimsPrincipal claims, SubImageAnnotationDTO subImageAnnotation) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var imageAnnotation = await dataContext
                .ImageAnnotations
                .Include(x => x.SubImageAnnotationGroups)
                .ThenInclude(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (imageAnnotation == null)
                return Results.NotFound("ImageAnnotation not found");

            if (imageAnnotation.BackgroundClassifications.Any(x => x.Users.Any(z => z.ID == userID)))
                return Results.BadRequest("User has already submitted a BackgroundClassification for this image");

            var count = subImageAnnotation.SubImages.Count;

            var subImageAnnotationGroupsEntity = imageAnnotation
                    .SubImageAnnotationGroups
                    .Where(x => x.SubImageAnnotations.Count == count).ToList();

            var user = dataContext.Users.Single(x => x.ID == userID);

            FindAndUpdateBestFitGroup(subImageAnnotationGroupsEntity, subImageAnnotation, user, 0.5f);

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

    public static double CalculateIOU(BoundingBoxDTO boxA, BoundingBoxDTO boxB)
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

    private static void FindAndUpdateBestFitGroup(List<SubImageAnnotationGroupEntity> subImageAnnotationGroups, SubImageAnnotationDTO subImageAnnotation, UserEntity currentUser, float iouThreshold)
    {
        SubImageAnnotationGroupEntity bestFitGroup = null;
        double maxMinIoU = -1;

        foreach (var group in subImageAnnotationGroups)
        {
            // Convert SubImageAnnotationDTO to a list of BoundingBoxDTO objects
            List<BoundingBoxDTO> newBoundingBoxes = subImageAnnotation.SubImages.ToList();

            // Calculate the cost matrix
            int n = group.SubImageAnnotations.Count;
            int m = newBoundingBoxes.Count;
            double[,] doubleCostMatrix = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    BoundingBoxDTO oldBoxDTO = new() { X = group.SubImageAnnotations.ElementAt(i).X, Y = group.SubImageAnnotations.ElementAt(i).Y, Width = group.SubImageAnnotations.ElementAt(i).Width, Height = group.SubImageAnnotations.ElementAt(i).Height };
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

                // Update the average bounding boxes for the best fit group
                int newUserCount = group.Users.Count + 1;
                for (int i = 0; i < n; i++)
                {
                    var oldBox = group.SubImageAnnotations.ElementAt(i);
                    var newBox = newBoundingBoxes[assignment[i]];

                    oldBox.X = (uint)((oldBox.X * (newUserCount - 1) + newBox.X) / newUserCount);
                    oldBox.Y = (uint)((oldBox.Y * (newUserCount - 1) + newBox.Y) / newUserCount);
                    oldBox.Width = (uint)((oldBox.Width * (newUserCount - 1) + newBox.Width) / newUserCount);
                    oldBox.Height = (uint)((oldBox.Height * (newUserCount - 1) + newBox.Height) / newUserCount);
                }

                // Update the Users collection if necessary
                if (!group.Users.Contains(currentUser))
                {
                    group.Users.Add(currentUser);
                }
            }
        }

        if (bestFitGroup != null)
        {
            // Update the bestFitGroup's SubImageAnnotations and Users
            bestFitGroup.SubImageAnnotations = subImageAnnotation.SubImages.Select(x => new SubImageAnnotationEntity()
            {
                X = x.X,
                Y = x.Y,
                Width = x.Width,
                Height = x.Height
            }).ToList();
            bestFitGroup.Users.Add(currentUser);
        }
        else
        {
            // Create a new SubImageAnnotationGroupEntity if no suitable group is found
            SubImageAnnotationGroupEntity newGroup = new SubImageAnnotationGroupEntity()
            {
                SubImageAnnotations = subImageAnnotation.SubImages.Select(x => new SubImageAnnotationEntity()
                {
                    X = x.X,
                    Y = x.Y,
                    Width = x.Width,
                    Height = x.Height
                }).ToList(),
                Users = new List<UserEntity>() { currentUser }
            };
            // Add the new group to the subImageAnnotationGroups list
            subImageAnnotationGroups.Add(newGroup);
        }
    }
}