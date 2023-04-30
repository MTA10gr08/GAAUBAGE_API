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
        app.MapGet("/usergoal", (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.Find(userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var now = DateTime.UtcNow;
            var startOfDayUtc = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

            List<UserGoalDTO> userGoals = new()
            {
                new()
                {
                    TaskType = "CC",
                    TotalToDo = 10u,
                    Done = (uint)(dataContext.Users.Find(userID)?.BackgroundClassifications.Count(x => EF.Functions.DateDiffDay(x.Created, startOfDayUtc) >= 0) ?? 0),
                },
                new()
                {
                    TaskType = "SI",
                    TotalToDo = 10u,
                    Done = (uint)(dataContext.Users.Find(userID)?.SubImageAnnotationGroups.Count(x => EF.Functions.DateDiffDay(x.Created, startOfDayUtc) >= 0) ?? 0),
                },
                new()
                {
                    TaskType = "TC",
                    TotalToDo = 5u,
                    Done = (uint)(dataContext.Users.Find(userID)?.TrashSubCategories.Count(x => EF.Functions.DateDiffDay(x.Created, startOfDayUtc) >= 0) ?? 0),
                },
                new()
                {
                    TaskType = "Se",
                    TotalToDo = 5u,
                    Done = (uint)(dataContext.Users.Find(userID)?.Segmentations.Count(x => EF.Functions.DateDiffDay(x.Created, startOfDayUtc) >= 0) ?? 0),
                }
            };

            return Results.Ok(userGoals);
        }).Produces<List<UserGoalDTO>>();
    }
}