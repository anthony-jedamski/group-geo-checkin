namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;



[ApiController]
[Route("[controller]")]
public class CheckInController : ControllerBase
{
    /*
        | Method | URL                                     | Description                |
    | ------ | --------------------------------------- | -------------------------- |
    | POST   | `/api/GroupCheckIn/register`            | Register a user            |
    | POST   | `/api/GroupCheckIn/group`               | Create a group             |
    | POST   | `/api/GroupCheckIn/checkin`             | Submit a location check-in |
    | GET    | `/api/GroupCheckIn/checkins/group/{id}` | Get group check-ins        |
    */
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    private readonly CheckInContext _context;
    public CheckInController(CheckInContext context)
    {
        _context = context;
    }

    // POST: api/GroupCheckIn/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
            return BadRequest("Username is required.");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    // POST: api/GroupCheckIn/group
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] Group group)
    {
        if (string.IsNullOrWhiteSpace(group.Name))
            return BadRequest("Group name is required.");

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return Ok(group);
    }

    // POST: api/GroupCheckIn/checkin
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] LocationCheckIn checkIn)
    {
        checkIn.Timestamp = DateTime.UtcNow;
        _context.LocationCheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        return Ok(checkIn);
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

    // GET: api/GroupCheckIn/checkins/group/1
    [HttpGet("checkins/group/{groupId}")]
    public async Task<IActionResult> GetCheckInsByGroup(int groupId)
    {
        var checkIns = await _context.LocationCheckIns
            .Include(c => c.User)
            .Where(c => c.GroupId == groupId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();

        return Ok(checkIns);
    }
}