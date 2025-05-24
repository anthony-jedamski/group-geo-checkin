namespace GeoCheckInBackend.Models;

public class Group
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}