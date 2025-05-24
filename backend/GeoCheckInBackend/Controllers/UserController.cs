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

[ApiController]
[Route("[controller]")]
public class CheckInController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    private readonly CheckInContext _context;
    private readonly IGroupService _groupService;
    public CheckInController(CheckInContext context, IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    /// <summary>
    /// Registers a user to a group.
    /// If the group does not exist, it creates a new group with the specified name.
    /// If the user does not exist in the group, it adds the user to the group.
    /// If the user already exists in the group, it does nothing.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>/
    [HttpPost("register/user/{userName}/group/{groupName}")]
    public async Task<IActionResult> RegisterUser(string userName, string? groupName = null)
    {
        var group = await _groupService.AddUserToGroupAsync(userName, groupName);
        _groupLocations.TryAdd(userName, new UserLocation());
        _context.Users.Add(group.Users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))!);
        if (group.Users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)) is null)
        {
            return BadRequest("User not found in the group.");
        }
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
        return Ok(group.Users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Records a check-in for a user at a specific location.
    /// The check-in includes the user's ID, location, and timestamp.
    /// </summary>
    /// <param name="checkIn"></param>
    /// <returns></returns>
    [HttpPost("checkin")]
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
    [HttpDelete("checkin/{id}")]
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
    [HttpGet("checkins/user/{userId}")]
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
    /// Removes a user from a group.
    /// If the user does not exist in the group, it returns a NotFound result.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    [HttpDelete("user/{userName}/group/{groupName}")]
    public async Task<IActionResult> RemoveUserFromGroup(string userName, string groupName)
    {
        try
        {
            var group = await _groupService.RemoveUserFromGroupAsync(userName, groupName);
            if (group == null)
            {
                return NotFound(new { Message = "User or group not found." });
            }
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a user's group by adding them to a new group.
    /// If the user does not exist, it creates a new group with the specified name.
    /// If the new group name is the same as the current group name, it does nothing.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <param name="newGroupName"></param>
    /// <returns></returns>
    [HttpPatch("user/{userName}/{groupName}/{newGroupName}")]
    public async Task<IActionResult> UpdateUserGroup(string userName, string groupName, string newGroupName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(newGroupName))
        {
            return BadRequest("User name, group name, and new group name are required.");
        }

        try
        {
            var group = await _groupService.AddUserToGroupAsync(userName, newGroupName);
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}