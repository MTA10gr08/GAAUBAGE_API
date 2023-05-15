using System.Security.Claims;
using API.DTOs.Annotation;
using API.DTOs.Gamification;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class CommunityGoalEndpoints
{
    public static void MapCommunityGoalEndpoints(this WebApplication app)
    {
        app.MapGet("/communitygoal", async (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
                return Results.BadRequest("Invalid user ID format");

            var user = dataContext.Users.Find(userID);
            if (user == null)
                return Results.BadRequest("User not found");

            var now = DateTimeOffset.Now.AddHours(-11);
            int diffToMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfWeekUtc = now.AddDays(-diffToMonday).Date.AddHours(11);

            var backgroundClassifications = await dataContext.BackgroundClassifications.Include(x => x.Users).ToListAsync();
            var SubImageGroups = await dataContext.SubImageGroups.Include(x => x.Users).ToListAsync();
            var TrashSubCategories = await dataContext.TrashSubCategories.Include(x => x.Users).ToListAsync();
            var Segmentations = await dataContext.Segmentations.ToListAsync();

            return (now.Day % 4) switch
            {
                0 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "CC",
                    TotalToDo = 200u,
                    DoneAll = (uint)backgroundClassifications.Count(x => x.Created >= startOfWeekUtc),
                    DoneYou = (uint)backgroundClassifications.Count(x => x.Created >= startOfWeekUtc && x.Users.Any(y => y.ID == userID))
                }),
                1 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "SI",
                    TotalToDo = 200u,
                    DoneAll = (uint)SubImageGroups.Count(x => x.Created >= startOfWeekUtc),
                    DoneYou = (uint)SubImageGroups.Count(x => x.Created >= startOfWeekUtc && x.Users.Any(y => y.ID == userID))
                }),
                2 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "TC",
                    TotalToDo = 100u,
                    DoneAll = (uint)TrashSubCategories.Count(x => x.Created >= startOfWeekUtc),
                    DoneYou = (uint)TrashSubCategories.Count(x => x.Created >= startOfWeekUtc && x.Users.Any(y => y.ID == userID))
                }),
                3 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "Se",
                    TotalToDo = 100u,
                    DoneAll = (uint)Segmentations.Count(x => x.Created >= startOfWeekUtc),
                    DoneYou = (uint)Segmentations.Count(x => x.Created >= startOfWeekUtc && x.User.ID == userID)
                }),
                _ => Results.BadRequest("How did you get here?"),
            };
        }).Produces<CommunityGoalDTO>();
    }
}