namespace GeoCheckInBackend.Models;
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}