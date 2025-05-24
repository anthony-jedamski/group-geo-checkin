namespace GeoCheckInBackend.Models
{
    public class UserCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int GroupId { get; set; }
    }
}