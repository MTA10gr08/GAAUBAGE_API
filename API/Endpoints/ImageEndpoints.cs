using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        app.MapPost("/image", async (ImageDTO image, DataContext dataContext) =>
        {
            var createdImage = dataContext.Images.Add(new ImageEntity { URI = image.URI }).Entity;

            try { await dataContext.SaveChangesAsync(); }
            catch (DbUpdateException ex) { return Results.BadRequest(ex.Message); }

            return Results.Ok(createdImage);
        }).Produces<ImageDTO>();

        app.MapGet("/image/{id}", (Guid id, DataContext dataContext) =>
        {
            var imageEntity = dataContext.Images.FirstOrDefault(x => x.Id == id);
            if (imageEntity != null)
            {
                var imageDTO = new ImageDTO
                {
                    Id = imageEntity.Id,
                    URI = imageEntity.URI,
                };
                return Results.Ok(imageDTO);
            }
            return Results.BadRequest();
        }).Produces<ImageDTO>();

        app.MapGet("/image", (DataContext dataContext) =>
        {
            var imageEntities = dataContext.Images.ToList();
            var imageDTOs = imageEntities.ConvertAll(x => new ImageDTO
            {
                Id = x.Id,
                URI = x.URI,
            });
            return Results.Ok(imageDTOs);
        }).Produces<List<ImageDTO>>();
    }
}