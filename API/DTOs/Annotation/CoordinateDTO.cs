namespace API.DTOs.Annotation;
public class CoordinateDTO
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double X => Longitude;
    public double Y => Latitude;
}