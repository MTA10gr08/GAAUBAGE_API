using API.DTOs.Annotation;
using API.Entities;

namespace API.Endpoints;
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/user", (UserDTO user, DataContext dataContext, TokenProvider tokenProvider) =>
        {
            var createdUser = dataContext.Users.Add(new UserEntity { Alias = user.Alias, Tag = user.Tag }).Entity;
            dataContext.SaveChanges();
            return Results.Ok(tokenProvider.GenerateToken(createdUser.Id, Role.User, DateTime.Now.AddYears(1)));
        });

        app.MapGet("/user/{id}", (Guid id, DataContext dataContext) =>
        {
            var userEntity = dataContext.Users.FirstOrDefault(x => x.Id == id);
            if (userEntity != null)
            {
                var userDTO = new UserDTO
                {
                    Id = userEntity.Id,
                    Alias = userEntity.Alias,
                    Tag = userEntity.Tag,
                    UserContextCategoryIds = userEntity.UserContextCategories.Select(x => x.Id).ToList(),
                    UserBackgroundContextIds = userEntity.UserBackgroundContexts.Select(x => x.Id).ToList(),
                    UserTrashCountIds = userEntity.UserTrashCounts.Select(x => x.Id).ToList(),
                    UserTrashBoundingBoxIds = userEntity.UserTrashBoundingBoxes.Select(x => x.Id).ToList(),
                    UserTrashSuperCategoryIds = userEntity.UserTrashSuperCategories.Select(x => x.Id).ToList(),
                    UserTrashCategoryIds = userEntity.UserTrashCategories.Select(x => x.Id).ToList(),
                    UserSegmentationIds = userEntity.UserSegmentations.Select(x => x.Id).ToList(),
                };
                return Results.Ok(userDTO);
            }
            return Results.BadRequest();
        });
    }
}