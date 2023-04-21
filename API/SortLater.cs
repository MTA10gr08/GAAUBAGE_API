using System.Net.Mime;
using Accord.Math;
using Accord.MachineLearning;
using API.DTOs.Annotation;

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