using System.Net;
using System.Net.Http.Headers;
using API.DTOs.Annotation;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class DebugEndpoints
{
    public static void MapDebugEndpoints(this WebApplication app, WebApplicationBuilder builder)
    {
        app.MapPost("/debug/insertsampledata", async (IOptions<AppSettings> IappSettings) =>
        {
            var appSettings = IappSettings.Value;
            var baseAddress = something.GetBaseAddress(builder);

            if (string.IsNullOrEmpty(baseAddress)) return Results.BadRequest("Could not obtain base address for seeding data");

            var usersToSeed = new string[]
            {
                "Martin",
                "Marcus",
                "Anne",
                "Rick",
                "Freja",
                "Mads",
                "Thomas",
                "Oscar",
                "Mikkel",
                "David",
                "Anton",
                "Annie",
                "Veronica",
                "Morten",
                "Alexander",
                "Julien",
                "Felix",
                "Sean",
                "Jack",
                "Mark",
                "Marzia",
                "Mette",
                "Mia",
                "Maja",
                "Jakob",
                "Esben",
                "Tobias",
                "Patrick"
            }
                .Select(x => new UserDTO { Alias = x, Tag = "SampleData" })
                .ToList();

            var clients = new List<HttpClient>();

            foreach (var user in usersToSeed)
            {
                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
                var response = await client.PostAsJsonAsync("/users/admin", user);
                var token = await response.Content.ReadFromJsonAsync<string>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                clients.Add(client);
            }

            var imagesToSeed = new List<ImageDTO> {
                new () { URI = "https://i.imgur.com/WGU4wHO.jpeg" },
                new () { URI = "https://i.imgur.com/g7N92jb.jpeg" },
                new () { URI = "https://i.imgur.com/Gy6TYdF.jpeg" },
                new () { URI = "https://i.imgur.com/oODM6FO.jpeg" },
                new () { URI = "https://i.imgur.com/rnt6yD8.jpeg" },
                new () { URI = "https://i.imgur.com/ibeDGNm.jpeg" },
                new () { URI = "https://i.imgur.com/w5svg7E.jpeg" },
                new () { URI = "https://i.imgur.com/ZDq7mK8.jpeg" },
                new () { URI = "https://i.imgur.com/KfnBEdK.jpeg" },
                new () { URI = "https://i.imgur.com/TGBulEN.jpeg" },
                new () { URI = "https://i.imgur.com/FVVmQjy.png" },
                new () { URI = "https://i.imgur.com/dDMQAwJ.png" },
                new () { URI = "https://i.imgur.com/Vk0VMUp.png" },
                new () { URI = "https://i.imgur.com/Yl287Se.png" },
                new () { URI = "https://i.imgur.com/kmYKvQS.png" },
                new () { URI = "https://i.imgur.com/GjNlsuI.png" },
                new () { URI = "https://i.imgur.com/CRMIYEa.png" },
                new () { URI = "https://i.imgur.com/MX9Bwlj.png" },
                new () { URI = "https://i.imgur.com/fWuAcYo.png" },
                new () { URI = "https://i.imgur.com/bhEC4L6.png" },
                new () { URI = "https://i.imgur.com/cIiQJpQ.png" },
                new () { URI = "https://i.imgur.com/zKJnI5x.png" },
                new () { URI = "https://i.imgur.com/Gzj3qAm.jpeg" },
                new () { URI = "https://i.imgur.com/Yy8K080.jpeg" },
                new () { URI = "https://i.imgur.com/oJP7KTa.jpeg" },
                new () { URI = "https://i.imgur.com/Vf6EFrF.jpeg" },
                new () { URI = "https://i.imgur.com/ICTSqlJ.jpeg" },
                new () { URI = "https://i.imgur.com/i7NF0Gj.jpeg" },
                new () { URI = "https://i.imgur.com/15Tf63x.jpeg" },
                new () { URI = "https://i.imgur.com/2CRNgwI.jpeg" },
                new () { URI = "https://i.imgur.com/Y0Tg0O5.jpeg" },
                new () { URI = "https://i.imgur.com/T3IzJzx.jpeg" },
                new () { URI = "https://i.imgur.com/I9SuQFw.jpeg" },
                new () { URI = "https://i.imgur.com/b8yO68v.jpeg" },
                new () { URI = "https://i.imgur.com/Twnqtay.png" },
                new () { URI = "https://i.imgur.com/MqgOs2F.jpeg" },
                new () { URI = "https://i.imgur.com/51yuyud.jpeg" },
                new () { URI = "https://i.imgur.com/sYyHgoz.jpeg" },
                new () { URI = "https://i.imgur.com/7MbehYb.jpeg" },
                new () { URI = "https://i.imgur.com/UlXOrHD.jpeg" },
                new () { URI = "https://i.imgur.com/rG2qynR.jpeg" },
                new () { URI = "https://i.imgur.com/uyYDsKw.png" },
                new () { URI = "https://i.imgur.com/8rb0uMK.jpeg" },
                new () { URI = "https://i.imgur.com/XjsqBnh.png" },
                new () { URI = "https://i.imgur.com/dihgk6X.jpeg" },
                new () { URI = "https://i.imgur.com/PzBCny8.jpeg" }
            };

            Random rng = new();
            for (int i = imagesToSeed.Count - 1; i >= 0; i--)
            {
                int k = rng.Next(i + 1);
                (imagesToSeed[i], imagesToSeed[k]) = (imagesToSeed[k], imagesToSeed[i]);
            }

            foreach (var image in imagesToSeed)
            {
                var response = await clients[rng.Next(0, clients.Count)].PostAsJsonAsync("/images", new List<ImageDTO>() { image });
                Console.WriteLine(response);
            }

            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine();
                var client = clients[rng.Next(0, clients.Count)];
                switch (rng.Next(0, 5))
                {
                    case 0:
                        Console.Write("BC " + await doBackgroundClassificationAsync(client, rng));
                        break;
                    case 1:
                        Console.Write("CC " + await doContextClassificationAsync(client, rng));
                        break;
                    case 2:
                        Console.Write("SI " + await doSubImageAsync(client, rng));
                        break;
                    case 3:
                        Console.Write("TC " + await doTrashSuperCategoryAsync(client, rng));
                        break;
                    case 4:
                        Console.Write("Se " + await doSegmentationAsync(client));
                        break;
                }
                Console.Write(" " + i);
            }

            foreach (var client in clients)
            {
                client.Dispose();
            }

            return Results.Ok("Data seeded successfully.");
        }).AllowAnonymous().RequireHost("localhost");

        static async Task<bool> doBackgroundClassificationAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/backgroundclassifications/next");
            //Console.WriteLine(response);
            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();
            //Console.WriteLine(responseContent == null ? "responseContent missing" : "responseContent found");
            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"/imageannotations/{responseContent.ID}/backgroundclassifications", new BackgroundClassificationDTO
            {
                BackgroundClassificationLabels = rng.Next(0, 5) >= 1 ? new string[] { "option 1" } : new string[] { "option 1", "option 2" }
            });
            //Console.WriteLine(response);
            return true;
        }

        static async Task<bool> doContextClassificationAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/contextclassifications/next");
            //Console.WriteLine(response);
            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();
            //Console.WriteLine(responseContent == null ? "responseContent missing" : "responseContent found");
            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"/imageannotations/{responseContent.ID}/contextclassifications", new ContextClassificationDTO
            {
                ContextClassificationLabel = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2"
            });
            //Console.WriteLine(response);
            return true;
        }

        static async Task<bool> doSubImageAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/subimages/next");
            //Console.WriteLine(response);
            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();
            //Console.WriteLine(responseContent == null ? "responseContent missing" : "responseContent found");
            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"/imageannotations/{responseContent.ID}/subimages", new SubImageAnnotationGroupDTO
            {
                SubImageAnnotations = rng.Next(0, 5) >= 1 ? new List<SubImageAnnotationDTO>() {
                    new SubImageAnnotationDTO {
                        X = 250,
                        Y = 400,
                        Width = 300,
                        Height = 420
                    },
                    new SubImageAnnotationDTO {
                        X = 1100,
                        Y = 950,
                        Width = 200,
                        Height = 330
                    }
                } : new List<SubImageAnnotationDTO>() {
                    new SubImageAnnotationDTO {
                        X = 600,
                        Y = 530,
                        Width = 560,
                        Height = 220
                    }
                }
            });
            //Console.WriteLine(response);
            return true;
        }

        static async Task<bool> doTrashSuperCategoryAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("/imageannotations/subimageannotations/trashsupercategories/next");
            //Console.WriteLine(response);
            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<SubImageAnnotationDTO>();
            //Console.WriteLine(responseContent == null ? "responseContent missing" : "responseContent found");
            if (responseContent == null) return false;


            var Super = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2";
            var Sub = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2";

            response = await client.PostAsJsonAsync($"imageannotations/subimageannotations/{responseContent.ID}/trashsupercategories", new TrashSuperCategoryDTO
            {
                TrashSuperCategoryLabel = Super
            });
            //Console.WriteLine(response);

            response = await client.PostAsJsonAsync($"imageannotations/subimageannotations/{responseContent.ID}/trashsubcategories", new TrashSuperCategoryDTO
            {
                TrashSuperCategoryLabel = Sub
            });
            //Console.WriteLine(response);
            return true;
        }

        static async Task<bool> doSegmentationAsync(HttpClient client)
        {
            var response = await client.GetAsync("/imageannotations/subimageannotations/segmentations/next");
            //Console.WriteLine(response);
            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<SubImageAnnotationDTO>();
            //Console.WriteLine(responseContent == null ? "responseContent missing" : "responseContent found");
            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"imageannotations/subimageannotations/{responseContent.ID}/segmentations", new SegmentationDTO
            {
                SegmentationMultiPolygon = new MultiPolygonDTO
                {
                    Polygons = new List<PolygonDTO>(){
                        new PolygonDTO(){
                            Shell = new LinearRingDTO(){
                                Coordinates = new List<CoordinateDTO>(){
                                    new CoordinateDTO(){
                                        Longitude = 0,
                                        Latitude = 0
                                    },new CoordinateDTO(){
                                        Longitude = 1,
                                        Latitude = 0
                                    },new CoordinateDTO(){
                                        Longitude = 0,
                                        Latitude = 1
                                    },new CoordinateDTO(){
                                        Longitude = 0,
                                        Latitude = 0
                                    },
                                }
                            },
                            Holes = new List<LinearRingDTO>()
                        }
                    }
                }
            });
            //Console.WriteLine(response);
            return true;
        }
    }
}