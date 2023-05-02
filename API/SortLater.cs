using System.Net.Mime;
using Accord.Math;
using Accord.MachineLearning;
using API.DTOs.Annotation;
using NetTopologySuite.Geometries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using API.Entities;
using System.Security.Claims;

public class Rectangle
{
    public Rectangle(uint x, uint y, uint width, uint height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public float CalculateIoU(Rectangle other)
    {
        float x1 = Math.Max(X, other.X);
        float y1 = Math.Max(Y, other.Y);
        float x2 = Math.Min(X + Width, other.X + other.Width);
        float y2 = Math.Min(Y + Height, other.Y + other.Height);

        float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        float union = (Width * Height) + (other.Width * other.Height) - intersection;

        return intersection / union;
    }
}
public static class something
{
    public static string? GetBaseAddress(WebApplicationBuilder builder)
    {
        var address = builder.Configuration.GetValue<string>("ASPNETCORE_URLS");
        if (string.IsNullOrEmpty(address)) address = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Url").Value;
        if (string.IsNullOrEmpty(address)) address = builder.Configuration.GetSection("profiles:MyApi:applicationUrl").Value;

        if (!string.IsNullOrEmpty(address)) address = address.Split(";", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        return address;
    }

    public static bool ValidateUserAndRole(DataContext dataContext, ClaimsPrincipal claims, Role requiredRole, out UserEntity? user, out IResult result)
    {
        user = null;
        result = Results.Ok();
        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        var userRoleClaim = claims.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || userRoleClaim == null || userRoleClaim?.Value != requiredRole)
        {
            result = Results.Unauthorized();
            return false;
        }

        if (!Guid.TryParse(userIdClaim.Value, out Guid userID))
        {
            result = Results.BadRequest("Invalid user ID format");
            return false;
        }

        user = dataContext.Users.Find(userID);
        if (user == null)
        {
            result = Results.BadRequest("User not found");
            return false;
        }

        return true;
    }
}

public static class GeometryConverter
{
    public static MultiPolygon ToMultiPolygon(MultiPolygonDTO multiPolygonDto)
    {
        var polygons = new List<Polygon>();
        foreach (var polygonDto in multiPolygonDto.Polygons)
        {
            polygons.Add(ToPolygon(polygonDto));
        }
        return new MultiPolygon(polygons.ToArray());
    }

    private static Polygon ToPolygon(PolygonDTO polygonDto)
    {
        var shell = ToLinearRing(polygonDto.Shell);
        var holes = new List<LinearRing>();
        foreach (var holeDto in polygonDto.Holes)
        {
            holes.Add(ToLinearRing(holeDto));
        }
        return new Polygon(shell, holes.ToArray());
    }

    private static LinearRing ToLinearRing(LinearRingDTO linearRingDto)
    {
        var coordinates = new List<Coordinate>();
        foreach (var coordinateDto in linearRingDto.Coordinates)
        {
            coordinates.Add(ToCoordinate(coordinateDto));
        }
        return new LinearRing(coordinates.ToArray());
    }

    private static Coordinate ToCoordinate(CoordinateDTO coordinateDto)
    {
        return new Coordinate(coordinateDto.Longitude, coordinateDto.Latitude);
    }
}

public static class SemaphoreSlimExtensions
{
    private sealed class AsyncSemaphoreReleaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public AsyncSemaphoreReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public async ValueTask DisposeAsync()
        {
            _semaphore.Release();
            await ValueTask.CompletedTask;
        }
    }

    public static async ValueTask<IAsyncDisposable> WaitAsyncDisposable(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new AsyncSemaphoreReleaser(semaphore);
    }
}

public static class EnumerableExtensions
{
    public static T WeightedRandom<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= new Random();

        int count = source.Count();
        double weightSum = count * (count + 1) / 2.0;

        double[] probabilities = new double[count];
        for (int i = 0; i < count; i++)
        {
            probabilities[i] = (count - i) / weightSum;
        }

        double randomValue = random.NextDouble();
        double cumulativeProbability = 0.0;

        for (int i = 0; i < count; i++)
        {
            cumulativeProbability += probabilities[i];

            if (randomValue < cumulativeProbability)
            {
                return source.ElementAt(i);
            }
        }

        // This line should never be reached if the probabilities are correct.
        throw new InvalidOperationException("An unexpected error occurred while selecting a weighted random item.");
    }

    public static IEnumerable<T> WeightedRandomSubset<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= new Random();

        int count = source.Count();
        int subsetSize = random.Next(1, count + 1);

        var selectedItems = new List<T>();

        for (int i = 0; i < subsetSize; i++)
        {
            var remainingItems = source.Except(selectedItems);
            var selectedItem = remainingItems.WeightedRandom(random);
            selectedItems.Add(selectedItem);
        }

        return selectedItems;
    }
}