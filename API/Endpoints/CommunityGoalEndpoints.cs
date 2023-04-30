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
        app.MapGet("/communitygoal", (DataContext dataContext, ClaimsPrincipal claims) =>
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
            int diffToMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfWeekUtc = now.AddDays(-diffToMonday).Date.AddHours(12);

            return (DateTime.UtcNow.Day % 4) switch
            {
                0 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "CC",
                    TotalToDo = 200u,
                    DoneAll = (uint)dataContext.BackgroundClassifications.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0),
                    DoneYou = (uint)dataContext.BackgroundClassifications.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0 && x.Users.Any(y => y.ID == userID))
                }),
                1 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "SI",
                    TotalToDo = 200u,
                    DoneAll = (uint)dataContext.SubImageGroups.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0),
                    DoneYou = (uint)dataContext.SubImageGroups.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0 && x.Users.Any(y => y.ID == userID))
                }),
                2 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "TC",
                    TotalToDo = 100u,
                    DoneAll = (uint)dataContext.TrashSubCategories.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0),
                    DoneYou = (uint)dataContext.TrashSubCategories.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0 && x.Users.Any(y => y.ID == userID))
                }),
                3 => Results.Ok(new CommunityGoalDTO()
                {
                    TaskType = "Se",
                    TotalToDo = 100u,
                    DoneAll = (uint)dataContext.Segmentations.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0),
                    DoneYou = (uint)dataContext.Segmentations.Count(x => EF.Functions.DateDiffDay(x.Created, startOfWeekUtc) >= 0 && x.User.ID == userID)
                }),
                _ => Results.BadRequest("How did you get here?"),
            };
        }).Produces<CommunityGoalDTO>();
    }
}