namespace API.Entities;
public class RectangleEntity : BaseEntity
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public Guid? GTTrashBoundingBoxEntityId { get; set; }
    public GTTrashBoundingBoxEntity? GTTrashBoundingBoxEntity { get; set; }

    public Guid? UserTrashBoundingBoxEntityId { get; set; }
    public UserTrashBoundingBoxEntity? UserTrashBoundingBoxEntity { get; set; }
    public float CalculateIoU(RectangleEntity other)
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