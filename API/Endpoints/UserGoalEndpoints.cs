using System.Security.Claims;
using API.DTOs.Annotation;
using API.DTOs.Gamification;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class UserGoalEndpoints
{
    public static void MapUserGoalEndpoints(this WebApplication app)
    {
        app.MapGet("/usergoal", async (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = await dataContext.Users
            .Include(x => x.BackgroundClassifications)
            .Include(x => x.SubImageAnnotationGroups)
            .Include(x => x.TrashSubCategories)
            .Include(x => x.Segmentations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.ID == userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var startOfDayUtc = DateTimeOffset.Now.Date.AddHours(12);

            List<UserGoalDTO> userGoals = new()
            {
                new()
                {
                    TaskType = "CC",
                    TotalToDo = 10u,
                    Done = (uint)user.BackgroundClassifications.Count(x => x.Created >= startOfDayUtc),
                },
                new()
                {
                    TaskType = "SI",
                    TotalToDo = 10u,
                    Done = (uint)user.SubImageAnnotationGroups.Count(x => x.Created >= startOfDayUtc),
                },
                new()
                {
                    TaskType = "TC",
                    TotalToDo = 5u,
                    Done = (uint)user.TrashSubCategories.Count(x => x.Created >= startOfDayUtc),
                },
                new()
                {
                    TaskType = "Se",
                    TotalToDo = 5u,
                    Done = (uint)user.Segmentations.Count(x => x.Created >= startOfDayUtc),
                }
            };

            return Results.Ok(userGoals);
        }).Produces<List<UserGoalDTO>>();
    }
}