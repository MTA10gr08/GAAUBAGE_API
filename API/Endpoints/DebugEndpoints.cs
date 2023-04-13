using System.Net.Http.Headers;
using API.DTOs.Annotation;
using Microsoft.Extensions.Options;

namespace API.Endpoints;
public static class DebugEndpoints
{
    public static void MapDebugEndpoints(this WebApplication app, WebApplicationBuilder builder)
    {
        app.MapPost("/insertSampleData", async (IOptions<AppSettings> IappSettings) =>
        {
            var appSettings = IappSettings.Value;
            var baseAddress = something.GetBaseAddress(builder);

            if (string.IsNullOrEmpty(baseAddress)) return Results.BadRequest("Could not obtain base address for seeding data");

            using var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };

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
            int n = imagesToSeed.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (imagesToSeed[n], imagesToSeed[k]) = (imagesToSeed[k], imagesToSeed[n]);
            }

            foreach (var image in imagesToSeed)
            {
                await httpClient.PostAsJsonAsync("/images", image);
            }

            /*
            var usersToSeed = new List<UserDTO> {
                new (){
                    Alias = "Martin",
                    Tag = "SampleData"
                },
                new (){
                    Alias = "Marcus",
                    Tag = "SampleData"
                },
                new (){
                    Alias = "Anne",
                    Tag = "SampleData"
                }};

            var clients = new List<HttpClient>();

            foreach (var user in usersToSeed)
            {
                var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
                var test = await client.PostAsJsonAsync("/user", user);
                var response = await test.Content.ReadFromJsonAsync<string>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response);
                var awd = client.DefaultRequestHeaders.Authorization;
                clients.Add(client);
            }

            for (int i = 0; i < imagesToSeed.Count; i++)
            {
                string BackgroundCategory = appSettings.BackgroundCategories[new Random().Next(appSettings.BackgroundCategories.Length)];
                foreach (var client in clients)
                {
                    var response = await client.GetAsync("/backgroundclassification/next");
                    var backgroundClassificationDTO = await response.Content.ReadFromJsonAsync<ImageDTO>();
                    var responseresponse = await client.PostAsJsonAsync("/backgroundclassification", new BackgroundClassificationDTO
                    {
                        ImageId = backgroundClassificationDTO.Id,
                        BackgroundCategory = BackgroundCategory
                    });
                }
            }

            foreach (var client in clients)
            {
                client.Dispose();
            }*/

            return Results.Ok("Data seeded successfully.");
        }).AllowAnonymous().RequireHost("localhost");
    }
}