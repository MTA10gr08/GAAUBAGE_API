using System.Security.Claims;
using API.DTOs.Gamification;
using API.Entities;

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
                .Select(x => new
                {
                    x.ID,
                    x.Alias,
                    x.Score
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            // Find the position of the current user in the sorted list
            int currentUserSpot = sortedUsers.FindIndex(x => x.ID == userId) + 1;

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
        });
    }
}