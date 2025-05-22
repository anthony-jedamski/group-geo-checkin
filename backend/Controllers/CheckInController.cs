namespace GeoCheckInBackEnd.Controllers;

using GeoCheckInBackEnd.Models;
using System.Collections.Concurrent;

[ApiController]
[Route("[controller]")]
public class CheckInController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    [HttpPost("register")]
    public IActionResult Register([FromBody] UserLocation user)
    {
        _groupLocations.TryAdd(user.GroupName, new List<UserLocation>());
        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("checkin")]
    public IActionResult CheckIn([FromBody] UserLocation user)
    {
        if (userLocation == null || string.IsNullOrEmpty(user.UserId))
        {
            return BadRequest("Invalid user location data.");
        }

        user.Timestamp = DateTime.UtcNow;
        _groupLocations[user.GroupName].TryAdd(user);

        return Ok(new { Message = "Check-in successful." });
    }

    [HttpGet("group/{groupName}")]
    public IActionResult GetGroupLocations(string groupName)
    {
        if (_groupLocations.TryGetValue(groupName, out var locations))
        {
            return Ok(locations);
        }

        return NotFound(new { Message = "Group not found." });
    }
}