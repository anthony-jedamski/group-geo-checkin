
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
        if (checkIn == null)
        {
            return BadRequest(new { Message = "Check-in data is required." });
        }
        if (checkIn.User == null || checkIn.User.Id <= 0)
        {
            return BadRequest(new { Message = "User ID is required." });
        }

        checkIn.Timestamp = DateTime.UtcNow;
        try 
        {
            // Retrieve the user from the database
            var user = await _context.Users.FindAsync(checkIn.User.Id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Assign the user to the check-in
            checkIn.User = user;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing the check-in.", Error = ex.Message });
        }
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
        // Check if the user exists
        if (checkIn.User == null)
        {
            return BadRequest(new { Message = "Check-in does not have a valid user." });
        }
        // Check if the user is part of a group
        var user = await _context.Users.FindAsync(checkIn.User.Id);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        if (user.GroupId <= 0)
        {
            return BadRequest(new { Message = "User is not part of a group." });
        }
        // Delete the check-in
        if (checkIn.User.GroupId != user.GroupId)
        {
            return BadRequest(new { Message = "Check-in does not belong to the user's group." });
        }
        // Remove the check-in from the database
        if (checkIn.User.GroupId <= 0)
        {
            return BadRequest(new { Message = "User is not part of a valid group." });
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
        if (checkIns == null || !checkIns.Any())
        {
            return NotFound(new { Message = "No check-ins found for this user." });
        }
        if (checkIns.Any(c => c.User == null))
        {
            return BadRequest(new { Message = "Some check-ins do not have a valid user." });
        }
        if (checkIns.Any(c => c.User.GroupId <= 0))
        {
            return BadRequest(new { Message = "Some check-ins are not associated with a valid group." });
        }
        if (checkIns.Any(c => c.User.GroupId != checkIns.First().User.GroupId))
        {
            return BadRequest(new { Message = "Check-ins do not belong to the same group." });
        }
        if (checkIns.Any(c => c.User.GroupId <= 0))
        {
            return BadRequest(new { Message = "User is not part of a valid group." });
        }

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
        if (checkIns == null || !checkIns.Any())
        {
            return NotFound(new { Message = "No check-ins found for this group." });
        }
        if (checkIns.Any(c => c.User == null))
        {
            return BadRequest(new { Message = "Some check-ins do not have a valid user." });
        }
        if (checkIns.Any(c => c.User.GroupId <= 0))
        {
            return BadRequest(new { Message = "Some check-ins are not associated with a valid group." });
        }
        if (checkIns.Any(c => c.User.GroupId != groupId))
        {
            return BadRequest(new { Message = "Check-ins do not belong to the specified group." });
        }
        if (checkIns.Any(c => c.User.GroupId <= 0))
        {
            return BadRequest(new { Message = "User is not part of a valid group." });
        }
        return Ok(checkIns);
    }
}