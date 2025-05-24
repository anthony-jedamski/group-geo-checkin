namespace GeoCheckInBackend.Models;

public class RegisterUserRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; } = null;
}
