using Accord.Math.Optimization;

namespace API.Entities;
public class GTTrashCountEntity : BaseGTProcessingEntity<UserTrashCountEntity, GTTrashCountEntity, uint>
{
    public GTTrashCountEntity() : base() { }
    public Guid GTContextClassificationId { get; set; }
    public GTContextClassificationEntity GTContextClassification { get; set; } = null!;
    public ICollection<GTTrashBoundingBoxEntity> GTTrashBoundingBoxes { get; set; } = new List<GTTrashBoundingBoxEntity>();
    public ICollection<UserTrashBoundingBoxEntity> UserTrashBoundingBoxes { get; set; } = new List<UserTrashBoundingBoxEntity>();
    public GTTrashBoundingBoxEntity? Consensus { get; set; }
    public bool IsInProgress => GTTrashBoundingBoxes.Count > 0 && Consensus == null;
    public void AddUserProcessing(UserTrashBoundingBoxEntity userProcessing)
    {
        const double averageThreshold = 0.5;
        const double individualThreshold = 0.5;
        /*var groundTruth = GTTrashBoundingBoxes // Make use of the Hungarian algorithm to find the best match
            .Select(gt => new
            {
                GroundTruth = gt,
                AverageIoU = userProcessing.Data.Sum(userRect => gt.Data.Max(gtRect => GetCachedIoU(gtRect, userRect))) / Data,
                MinIoU = userProcessing.Data.Min(userRect => gt.Data.Max(gtRect => GetCachedIoU(gtRect, userRect)))
            })
            .Where(x => x.AverageIoU >= averageThreshold && x.MinIoU >= individualThreshold)
            .OrderByDescending(x => x.AverageIoU)
            .FirstOrDefault()?.GroundTruth;*/

        foreach (var gt in GTTrashBoundingBoxes)
        {
            var gtData = gt.Data.ToArray();
            var userData = userProcessing.Data.ToArray();
            double[][] matrix = new double[Data][];

            for (int i = 0; i < Data; i++)
            {
                matrix[i] = new double[Data];

                for (int j = 0; j < Data; j++)
                {
                    matrix[i][j] = GetCachedIoU(gtData[i], userData[j]);
                }
            }

            Console.WriteLine(matrix);

            var m = new Munkres(matrix);


            Console.WriteLine(m.MinCol);
            Console.WriteLine(m.MinRow);
        }


        GTTrashBoundingBoxEntity? groundTruth = null;
        if (groundTruth == null)
        {
            groundTruth = new GTTrashBoundingBoxEntity
            {
                GTTrashCount = this,
                GTTrashCountId = Id,
                Data = userProcessing.Data,
            };
            GTTrashBoundingBoxes.Add(groundTruth);
        }

        groundTruth.UserProcessings.Add(userProcessing);
        UserTrashBoundingBoxes.Add(userProcessing);

        int total = UserTrashBoundingBoxes.Count;
        double threshold = total * 0.75;
        Consensus = GTTrashBoundingBoxes.FirstOrDefault(x => x.UserProcessings.Count >= threshold);
    }

    private readonly Dictionary<(RectangleEntity, RectangleEntity), float> iouCache = new();

    private float GetCachedIoU(RectangleEntity a, RectangleEntity b)
    {
        (RectangleEntity first, RectangleEntity second) = a.GetHashCode() <= b.GetHashCode() ? (a, b) : (b, a);

        if (!iouCache.TryGetValue((first, second), out float iou))
        {
            iou = a.CalculateIoU(b);
            iouCache[(first, second)] = iou;
        }
        return iou;
    }
}
public class UserTrashCountEntity : BaseUserProcessingEntity<UserTrashCountEntity, GTTrashCountEntity, uint>
{
    public UserTrashCountEntity() : base() { }
}