using System.Security.Claims;
using API.DTOs.Gamification;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class LeaderboardEndpoints
{
    public static void MapLeaderboardEndpoints(this WebApplication app)
    {
        app.MapGet("/leaderboard", (DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Results.BadRequest("Invalid user ID format");

            // Sort the users by their scores in descending order
            var sortedUsers = dataContext.Users
                .Include(x => x.BackgroundClassifications)
                .Include(x => x.ContextClassifications)
                .Include(x => x.SubImageAnnotationGroups)
                .Include(x => x.TrashSubCategories)
                .Include(x => x.TrashSuperCategories)
                .Include(x => x.Segmentations)
                .AsEnumerable()
                .Select(x => new
                {
                    x.ID,
                    x.Alias,
                    x.Score
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            // Find the position of the current user in the sorted list
            int currentUserSpot = sortedUsers.FindIndex(x => x.ID == userId);

            // Create the LeaderboardDTO object with the correct position and entries
            var leaderboard = new LeaderboardDTO()
            {
                CurrentUserSpot = (uint)currentUserSpot,
                Entries = sortedUsers.ConvertAll(x => new LeaderboardDTO.Entry()
                {
                    Alias = x.Alias,
                    Score = x.Score
                })
            };

            return Results.Ok(leaderboard);
        }).AllowAnonymous();
    }
}