using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/users", async (UserDTO user, DataContext dataContext, TokenProvider tokenProvider) =>
        {
            var createdUser = dataContext.Users.Add(new UserEntity { Alias = user.Alias, Tag = dataContext.Users.Count(x => x.Tag == "Narr") < dataContext.Users.Count(x => x.Tag == "Blap") ? "Narr" : "Blap" }).Entity;
            await dataContext.SaveChangesAsync();
            return Results.Ok(tokenProvider.GenerateToken(createdUser.ID, Role.User, DateTime.Now.AddYears(1)));
        }).Produces<string>();

        app.MapPost("/users/admin", async (UserDTO user, DataContext dataContext, TokenProvider tokenProvider) =>
        {
            var createdUser = dataContext.Users.Add(new UserEntity { Alias = user.Alias, Tag = dataContext.Users.Count(x => x.Tag == "Narr") < dataContext.Users.Count(x => x.Tag == "Blap") ? "Narr" : "Blap" }).Entity;
            await dataContext.SaveChangesAsync();
            return Results.Ok(tokenProvider.GenerateToken(createdUser.ID, Role.Admin, DateTime.Now.AddYears(1)));
        }).Produces<string>().RequireHost("localhost");

        app.MapGet("/users", async (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null) return Results.Unauthorized();

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID)) return Results.BadRequest("Invalid user ID format");

            var user = await dataContext.Users.FindAsync(userID);
            if (user == null) return Results.BadRequest("User not found");

            return Results.Ok(await dataContext.Users
            .Include(x => x.BackgroundClassifications)
            .Include(x => x.TrashSubCategories)
            .Include(x => x.SubImageAnnotationGroups)
            .Include(x => x.Segmentations)
            .Select(userEntity => new UserDTO()
            {
                ID = userEntity.ID,
                Created = userEntity.Created,
                Updated = userEntity.Updated,
                Alias = userEntity.Alias,
                Tag = userEntity.Tag,
                Score = userEntity.Score,
                Level = userEntity.Level,
                Images = userEntity.Images.Select(x => x.ID).ToList(),
                Skipped = userEntity.VoteSkipped.Select(x => x.ID).ToList(),
                BackgroundClassifications = userEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
                ContextClassifications = userEntity.ContextClassifications.Select(x => x.ID).ToList(),
                SubImageGroups = userEntity.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                TrashSuperCategories = userEntity.TrashSuperCategories.Select(x => x.ID).ToList(),
                TrashSubCategories = userEntity.TrashSubCategories.Select(x => x.ID).ToList(),
                Segmentations = userEntity.Segmentations.Select(x => x.ID).ToList(),
            }).ToListAsync());
        });

        app.MapGet("/users/me", async (DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
            {
                return Results.BadRequest("Invalid user ID format");
            }

            var user = await dataContext.Users.FindAsync(userID);
            if (user == null)
            {
                return Results.BadRequest("User not found");
            }

            var userEntity = await dataContext.Users
                .Include(x => x.Images)
                .Include(x => x.VoteSkipped)
                .Include(x => x.BackgroundClassifications)
                .Include(x => x.ContextClassifications)
                .Include(x => x.SubImageAnnotationGroups)
                .Include(x => x.TrashSubCategories)
                .Include(x => x.TrashSuperCategories)
                .Include(x => x.Segmentations)
                .FirstOrDefaultAsync(x => x.ID == user.ID);

            if (userEntity != null)
            {
                var userDTO = new UserDTO
                {
                    ID = userEntity.ID,
                    Created = userEntity.Created,
                    Updated = userEntity.Updated,
                    Alias = userEntity.Alias,
                    Tag = userEntity.Tag,
                    Score = userEntity.Score,
                    Level = userEntity.Level,
                    Images = userEntity.Images.Select(x => x.ID).ToList(),
                    Skipped = userEntity.VoteSkipped.Select(x => x.ID).ToList(),
                    BackgroundClassifications = userEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
                    ContextClassifications = userEntity.ContextClassifications.Select(x => x.ID).ToList(),
                    SubImageGroups = userEntity.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                    TrashSuperCategories = userEntity.TrashSuperCategories.Select(x => x.ID).ToList(),
                    TrashSubCategories = userEntity.TrashSubCategories.Select(x => x.ID).ToList(),
                    Segmentations = userEntity.Segmentations.Select(x => x.ID).ToList(),
                };
                return Results.Ok(userDTO);
            }
            return Results.BadRequest();
        });
    }
}