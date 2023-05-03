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

            //var clients = new List<HttpClient>();
            var clients = new Dictionary<HttpClient, Task<bool>>();

            foreach (var user in usersToSeed)
            {
                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
                var response = await client.PostAsJsonAsync("/users/admin", user);
                var token = await response.Content.ReadFromJsonAsync<string>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                clients.Add(client, Task.FromResult(true));
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

            var imageTasks = new List<Task>();
            foreach (var image in imagesToSeed)
            {
                imageTasks.Add(clients.ElementAt(rng.Next(0, clients.Count)).Key.PostAsJsonAsync("/images", new List<ImageDTO>() { image }));
            }
            Task.WaitAll(imageTasks.ToArray());


            int sucesses = 0;
            for (int i = 0; i < 1000; i++)
            {
                var clientindex = Task.WaitAny(clients.Values.ToArray());
                var client = clients.ElementAt(clientindex);
                sucesses += client.Value.Result ? 1 : 0;
                switch (rng.Next(0, 6))
                {
                    case 0:
                        //Console.Write("BC ");
                        clients[client.Key] = doBackgroundClassificationAsync(client.Key, rng);
                        //await doBackgroundClassificationAsync(client, rng);
                        break;
                    case 1:
                        //Console.Write("CC ");
                        clients[client.Key] = doContextClassificationAsync(client.Key, rng);
                        //await doContextClassificationAsync(client, rng);
                        break;
                    case 2:
                        //Console.Write("SI ");
                        clients[client.Key] = doSubImageAsync(client.Key, rng);
                        //await doSubImageAsync(client, rng);
                        break;
                    case 3:
                        //Console.Write("SP ");
                        clients[client.Key] = doTrashSuperCategoryAsync(client.Key, rng);
                        //await doTrashSuperCategoryAsync(client, rng);
                        break;
                    case 4:
                        //Console.Write("SB ");
                        clients[client.Key] = doTrashSubCategoryAsync(client.Key, rng);
                        //await doTrashSubCategoryAsync(client, rng);
                        break;
                    case 5:
                        //Console.Write("Se ");
                        clients[client.Key] = doSegmentationAsync(client.Key, rng);
                        //await doSegmentationAsync(client);
                        break;
                }
                //Console.Write(1+i);
                //Console.WriteLine();
            }

            Task.WaitAll(clients.Values.ToArray());

            Console.WriteLine($"sucesses: {sucesses}");

            foreach (var client in clients)
            {
                client.Key.Dispose();
            }

            return Results.Ok("Data seeded successfully.");
        }).AllowAnonymous().RequireHost("localhost");

        static async Task<bool> doBackgroundClassificationAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/backgroundclassifications/next");

            if (response.StatusCode != HttpStatusCode.OK) return false;

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();

            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"/imageannotations/{responseContent.ID}/backgroundclassifications", new BackgroundClassificationDTO
            {
                BackgroundClassificationLabels = rng.Next(0, 5) >= 1 ? new string[] { "option 1" } : new string[] { "option 1", "option 2" }
            });

            return response.StatusCode == HttpStatusCode.OK;
        }

        static async Task<bool> doContextClassificationAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/contextclassifications/next");

            if (response.StatusCode != HttpStatusCode.OK) return await doBackgroundClassificationAsync(client, rng);

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();

            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"/imageannotations/{responseContent.ID}/contextclassifications", new ContextClassificationDTO
            {
                ContextClassificationLabel = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2"
            });

            return response.StatusCode == HttpStatusCode.OK;
        }

        static async Task<bool> doSubImageAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("imageannotations/subimages/next");

            if (response.StatusCode != HttpStatusCode.OK) return await doContextClassificationAsync(client, rng);

            var responseContent = await response.Content.ReadFromJsonAsync<ImageAnnotationDTO>();

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

            return response.StatusCode == HttpStatusCode.OK;
        }

        static async Task<bool> doTrashSubCategoryAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("/imageannotations/subimageannotations/trashsubcategories/next");

            if (response.StatusCode != HttpStatusCode.OK) return await doSubImageAsync(client, rng);

            var responseContent = await response.Content.ReadFromJsonAsync<SubImageAnnotationDTO>();

            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"imageannotations/subimageannotations/{responseContent.ID}/trashsubcategories", new TrashSubCategoryDTO
            {
                TrashSubCategoryLabel = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2"
            });

            return response.StatusCode == HttpStatusCode.OK;
        }

        static async Task<bool> doTrashSuperCategoryAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("/imageannotations/subimageannotations/trashsupercategories/next");

            if (response.StatusCode != HttpStatusCode.OK) return await doTrashSubCategoryAsync(client, rng);

            var responseContent = await response.Content.ReadFromJsonAsync<SubImageAnnotationDTO>();

            if (responseContent == null) return false;

            response = await client.PostAsJsonAsync($"imageannotations/subimageannotations/{responseContent.ID}/trashsupercategories", new TrashSuperCategoryDTO
            {
                TrashSuperCategoryLabel = rng.Next(0, 5) >= 1 ? "Option 1" : "Option 2"
            });

            return response.StatusCode == HttpStatusCode.OK;
        }

        static async Task<bool> doSegmentationAsync(HttpClient client, Random rng)
        {
            var response = await client.GetAsync("/imageannotations/subimageannotations/segmentations/next");

            if (response.StatusCode != HttpStatusCode.OK) return await doTrashSuperCategoryAsync(client, rng);

            var responseContent = await response.Content.ReadFromJsonAsync<SubImageAnnotationDTO>();
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

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}