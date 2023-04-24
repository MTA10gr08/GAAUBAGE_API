using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageAnnotationEndpoints
{
    public static void MapImageAnnotationEndpoints(this WebApplication app)
    {

        app.MapGet("/imageannotations/{id}", (Guid id, DataContext dataContext) =>
        {
            var imageEntity = dataContext.ImageAnnotations.FirstOrDefault(x => x.ID == id);
            if (imageEntity != null)
            {
                var imageAnnotationDTO = new ImageAnnotationDTO
                {
                    ID = imageEntity.ID,
                    Created = imageEntity.Created,
                    Updated = imageEntity.Updated,
                    Image = imageEntity.Image.ID,
                    BackgroundClassifications = imageEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
                    BackgroundClassificationConsensus = imageEntity.BackgroundClassificationConsensus?.ID,
                    ContextClassifications = imageEntity.ContextClassifications.Select(x => x.ID).ToList(),
                    ContextClassificationConsensus = imageEntity.ContextClassificationConsensus?.ID,
                    SubImages = imageEntity.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                    SubImagesConsensus = imageEntity.SubImageAnnotationGroupConsensus?.ID,
                    IsInProgress = imageEntity.IsInProgress,
                    IsComplete = imageEntity.IsComplete,
                };
                return Results.Ok(imageAnnotationDTO);
            }
            return Results.BadRequest();
        }).Produces<ImageAnnotationDTO>().RequireAuthorization();
    }
}