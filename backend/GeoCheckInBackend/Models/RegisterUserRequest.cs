namespace GeoCheckInBackend.Models;
public class RegisterUserRequest
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? GroupName { get; set; }
}
