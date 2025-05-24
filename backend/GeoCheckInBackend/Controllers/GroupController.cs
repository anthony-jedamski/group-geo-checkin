namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


public class GroupController : ControllerBase
{
    private readonly CheckInContext _context;
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();

    public GroupController(CheckInContext context)
    {
        _context = context;
    }

   [HttpPost("group/create")]
    public async Task<IActionResult> GroupCreate([FromBody] GroupCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Group name is required.");

        var allGroups = await _context.Groups.ToListAsync();

        if (allGroups.Any(g => g.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Ok("Group with this name already exists.");
        }

        int groupId = (allGroups.Max(g => (int?)g.Id) ?? 0) + 1;

        var group = new Group
        {
            Name = request.Name,
            Id = groupId,
            Users = new List<User>()
        };

        _groupLocations.TryAdd(request.Name, new UserLocation());

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return Ok(group);
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
            .Where(c => c.User.Group.Id == groupId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();

        return Ok(checkIns);
    }
}