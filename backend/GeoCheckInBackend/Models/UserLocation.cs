namespace GeoCheckInBackend.Models;

public class UserLocation
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}