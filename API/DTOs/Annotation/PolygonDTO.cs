namespace API.DTOs.Annotation;
public class PolygonDTO
{
    public LinearRingDTO Shell { get; set; } = null!;
    public List<LinearRingDTO> Holes { get; set; } = null!;
}