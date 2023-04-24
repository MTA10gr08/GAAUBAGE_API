using API.DTOs.Annotation;
using API.DTOs.Gamification;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class UserGoalEndpoints
{
    public static void MapUserGoalEndpoints(this WebApplication app)
    {
        app.MapGet("/UserGoal", (IOptions<AppSettings> IappSettings) =>
        {
            var task = DateTime.UtcNow.Day % 4;
            UserGoalDTO categories = new()
            {
                TaskType = task == 0 ? "CC" : task == 1 ? "SI" : task == 2 ? "TC" : "Seg",
                TotalToDo = 10,
                Done = 5
            };
            return Results.Ok(categories);
        }).Produces<CategoriesDTO>().AllowAnonymous();
    }
}