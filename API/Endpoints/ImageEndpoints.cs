using System.Net.Http;
using System.Security.Claims;
using API.DTOs.Annotation;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;
public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        app.MapPost("/images", async (List<ImageDTO> images, DataContext dataContext, ClaimsPrincipal claims) =>
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
            var userRoleClaim = claims.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || userRoleClaim == null || userRoleClaim?.Value != Role.Admin)
            {
                return Results.Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
            {
                return Results.BadRequest("Invalid user ID format");
            }

            var user = dataContext.Users.Find(userID);
            if (user == null)
                return Results.BadRequest("User not found");

            if (images.Count == 0)
                return Results.BadRequest("No images provided");

            if (images.Any(x => x.URI == null))
                return Results.BadRequest("One or more images have no URI");

            using var client = new HttpClient();
            var failedURLs = new List<string>();
            var imagesToAdd = new List<ImageEntity>();
            var annotationsToAdd = new List<ImageAnnotationEntity>();

            foreach (var image in images)
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(image.URI);

                    if (response.IsSuccessStatusCode)
                    {
                        string contentType = response.Content.Headers.ContentType.MediaType;

                        if (contentType.StartsWith("image/"))
                        {
                            var newImage = new ImageEntity { URI = image.URI, UserID = user.ID };
                            var newImageAnnotation = new ImageAnnotationEntity { Image = newImage };

                            imagesToAdd.Add(newImage);
                            annotationsToAdd.Add(newImageAnnotation);
                        }
                        else
                        {
                            failedURLs.Add(image.URI);
                        }
                    }
                    else
                    {
                        failedURLs.Add(image.URI);
                    }
                }
                catch (Exception ex)
                {
                    failedURLs.Add(image.URI);
                }
            }

            if (imagesToAdd.Count > 0)
            {
                dataContext.Images.AddRange(imagesToAdd);
                dataContext.ImageAnnotations.AddRange(annotationsToAdd);

                try
                {
                    await dataContext.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }

            if (failedURLs.Count > 0)
            {
                return Results.BadRequest(new { message = "Failed to add one or more images due to URL being unreachable or not leading to an image", failedURLs });
            }

            return Results.Ok();
        }).RequireAuthorization();

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