
namespace GeoCheckInBackend.Controllers;
using GeoCheckInBackend.Models;
using GeoCheckInBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GeoCheckInBackend.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/checkin")]
public class CheckInController : ControllerBase
{
    /// <summary>
    /// A dictionary to store user locations by their names.
    /// </summary>
    /// <remarks>
    /// This is used to keep track of user locations in memory.
    /// </remarks>

    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    private readonly CheckInContext _context;
    private readonly IGroupService _groupService;
    public CheckInController(CheckInContext context, IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    /// <summary>
    /// Records a check-in for a user at a specific location.
    /// The check-in includes the user's ID, location, and timestamp.
    /// </summary>
    /// <param name="checkIn"></param>
    /// <returns></returns>
    [HttpPost("location")]
    [ProducesResponseType(typeof(LocationCheckIn), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    public async Task<IActionResult> CheckIn([FromBody] LocationCheckIn checkIn)
    {
        checkIn.Timestamp = DateTime.UtcNow;
        _context.LocationCheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        return Ok(checkIn);
    }

    /// <summary>
    /// Deletes a check-in by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>/
    [HttpDelete("delete/checkinId/{id}")]
    public async Task<IActionResult> DeleteCheckIn(int id)
    {
        var checkIn = await _context.LocationCheckIns.FindAsync(id);
        if (checkIn == null)
        {
            return NotFound(new { Message = "Check-in not found." });
        }

        _context.LocationCheckIns.Remove(checkIn);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Check-in deleted successfully." });
    }

    /// <summary>
    /// Gets all check-ins for a specific user by their user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("get/user/{userId}")]
    public async Task<IActionResult> GetCheckInsByUser(int userId)
    {
        var checkIns = await _context.LocationCheckIns
            .Include(c => c.User)
            .Where(c => c.User.Id == userId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();

        return Ok(checkIns);
    }

    /// <summary>
    /// Gets all check-ins for a specific user by their user ID.
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    [HttpGet("get/groupid/{groupId}")]
    public async Task<IActionResult> GetCheckInsByGroup(int groupId)
    {
        var checkIns = await _context.LocationCheckIns
            .Include(c => c.User)
            .Where(c => c.User.GroupId == groupId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();

        return Ok(checkIns);
    }
}