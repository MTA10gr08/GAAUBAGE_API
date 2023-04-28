using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageAnnotationEndpoints
{
    public static void MapImageAnnotationEndpoints(this WebApplication app)
    {
        app.MapPost("/imageannotations/{id}/voteskip", (Guid id, DataContext dataContext) =>
        {
            var imageAnnotationEntity = dataContext.ImageAnnotations.FirstOrDefault(x => x.ID == id);
            return Results.BadRequest();
        }).Produces<ImageAnnotationDTO>().RequireAuthorization();

        app.MapGet("/imageannotations/{id}", (Guid id, DataContext dataContext) =>
        {
            var imageAnnotationEntity = dataContext.ImageAnnotations.FirstOrDefault(x => x.ID == id);
            if (imageAnnotationEntity != null)
            {
                var imageAnnotationDTO = new ImageAnnotationDTO
                {
                    ID = imageAnnotationEntity.ID,
                    Created = imageAnnotationEntity.Created,
                    Updated = imageAnnotationEntity.Updated,
                    Image = imageAnnotationEntity.ImageID,
                    Skipped = imageAnnotationEntity.VoteSkipped.Select(x => x.ID).ToList(),
                    BackgroundClassifications = imageAnnotationEntity.BackgroundClassifications.Select(x => x.ID).ToList(),
                    BackgroundClassificationConsensus = imageAnnotationEntity.BackgroundClassificationConsensus?.ID,
                    ContextClassifications = imageAnnotationEntity.ContextClassifications.Select(x => x.ID).ToList(),
                    ContextClassificationConsensus = imageAnnotationEntity.ContextClassificationConsensus?.ID,
                    SubImages = imageAnnotationEntity.SubImageAnnotationGroups.Select(x => x.ID).ToList(),
                    SubImagesConsensus = imageAnnotationEntity.SubImageAnnotationGroupConsensus?.ID,
                    IsInProgress = imageAnnotationEntity.IsInProgress,
                    IsComplete = imageAnnotationEntity.IsComplete,
                };
                return Results.Ok(imageAnnotationDTO);
            }
            return Results.BadRequest();
        }).Produces<ImageAnnotationDTO>().RequireAuthorization();
    }
}