using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        app.MapPost("/images", async (ImageDTO image, DataContext dataContext, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Results.BadRequest("Invalid user ID format");
            }

            var createdImage = dataContext.Images.Add(new ImageEntity { URI = image.URI, UserID = userId }).Entity;
            var createdImageAnnotation = dataContext.ImageAnnotations.Add(new ImageAnnotationEntity { ImageID = createdImage.ID }).Entity;
            createdImage.ImageAnnotationID = createdImageAnnotation.ID;

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok(createdImage.ID);
        }).Produces<Guid>().RequireAuthorization();

        app.MapGet("/images/{id}", (Guid id, DataContext dataContext) =>
        {
            var imageEntity = dataContext.Images.FirstOrDefault(x => x.ID == id);
            if (imageEntity != null)
            {
                var imageDTO = new ImageDTO
                {
                    ID = imageEntity.ID,
                    Created = imageEntity.Created,
                    Updated = imageEntity.Updated,
                    URI = imageEntity.URI,
                    User = imageEntity.UserID,
                    ImageAnnotation = imageEntity.ImageAnnotationID,
                    SubImageAnnotations = imageEntity.SubImageAnnotations.Select(x => x.ID).ToList(),
                };
                return Results.Ok(imageDTO);
            }
            return Results.BadRequest();
        }).Produces<ImageDTO>().RequireAuthorization();

        app.MapGet("/images", (DataContext dataContext) =>
        {
            var imageEntities = dataContext.Images.ToList();
            var imageDTOs = imageEntities.ConvertAll(x => new ImageDTO
            {
                ID = x.ID,
                Created = x.Created,
                Updated = x.Updated,
                URI = x.URI,
                User = x.UserID,
                ImageAnnotation = x.ImageAnnotationID,
                SubImageAnnotations = x.SubImageAnnotations.Select(x => x.ID).ToList(),
            });

            return Results.Ok(imageDTOs);
        }).Produces<List<ImageDTO>>().RequireAuthorization();
    }
}