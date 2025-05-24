/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/

namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
using GeoCheckInBackend.Services;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


public class GroupController : ControllerBase
{
    private readonly CheckInContext _context;
    private readonly IGroupService _groupService;
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();

    public GroupController(CheckInContext context, IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    /// <summary>
    /// Creates a new group with the specified name.
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    [HttpPost("group/create")]
    public async Task<IActionResult> GroupCreate([FromBody] string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return BadRequest("Group name is required.");

        var allGroups = await _context.Groups.ToListAsync();

        if (allGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
        {
            return Ok("Group with this name already exists.");
        }

        try
        {
            var group = await _groupService.CreateGroupAsync(groupName);
            return Ok(group);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the locations of all users in a specified group.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    [HttpGet("group/{groupName}")]
    public IActionResult GetGroupLocations(string groupName)
    {
        if (_groupLocations.TryGetValue(groupName, out var locations))
        {
            return Ok(locations);
        }

        return NotFound(new { Message = "Group not found." });
    }

    /// <summary>
    /// Gets all check-ins for a specific user by their user ID.
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Updates the location of a group.
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("group/{groupName}")]
    public IActionResult UpdateGroupLocation(string groupName, [FromBody] UserLocation location)
    {
        if (string.IsNullOrWhiteSpace(groupName) || location == null)
            return BadRequest("Invalid group name or location data.");

        if (_groupLocations.TryGetValue(groupName, out var existingLocation))
        {
            existingLocation.Latitude = location.Latitude;
            existingLocation.Longitude = location.Longitude;
            existingLocation.Timestamp = DateTime.UtcNow;

            _groupLocations[groupName] = existingLocation;

            return Ok(existingLocation);
        }

        return NotFound(new { Message = "Group not found." });
    }

    /// <summary>
    /// Deletes a group by its name.
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    [HttpDelete("group/{groupName}")]
    public async Task<IActionResult> DeleteGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return BadRequest("Group name is required.");

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

        if (group == null)
            return NotFound(new { Message = "Group not found." });

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        _groupLocations.TryRemove(groupName, out _);

        return Ok(new { Message = "Group deleted successfully." });
    }
}