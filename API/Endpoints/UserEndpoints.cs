using API.DTOs.Annotation;
using API.Entities;

namespace API.Endpoints;
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/users", (UserDTO user, DataContext dataContext, TokenProvider tokenProvider) =>
        {
            var createdUser = dataContext.Users.Add(new UserEntity { Alias = user.Alias, Tag = user.Tag }).Entity;
            dataContext.SaveChanges();
            return Results.Ok(tokenProvider.GenerateToken(createdUser.ID, Role.User, DateTime.Now.AddYears(1)));
        }).Produces<string>();

        app.MapPost("/users/admin", (UserDTO user, DataContext dataContext, TokenProvider tokenProvider) =>
        {
            var createdUser = dataContext.Users.Add(new UserEntity { Alias = user.Alias, Tag = user.Tag }).Entity;
            dataContext.SaveChanges();
            return Results.Ok(tokenProvider.GenerateToken(createdUser.ID, Role.Admin, DateTime.Now.AddYears(1)));
        }).Produces<string>().RequireHost("localhost");

        app.MapGet("/users/{id}", (Guid id, DataContext dataContext) =>
        {
            var userEntity = dataContext.Users.FirstOrDefault(x => x.ID == id);
            if (userEntity != null)
            {
                var userDTO = new UserDTO
                {
                    ID = userEntity.ID,
                    Alias = userEntity.Alias,
                    Tag = userEntity.Tag,
                    Images = userEntity.Images.Select(x => x.ID).ToList(),
                    BackgroundClassificationLabels = userEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
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