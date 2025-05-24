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
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    private readonly CheckInContext _context;
    private readonly IGroupService _groupService;
    public UserController(CheckInContext context, IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    [HttpGet("get/username/{userName}")]
    public async Task<IActionResult> GetUser(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return BadRequest("User name is required.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName, userName)); // For PostgreSQL

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Username and email are required.");
        }

        var group = await _groupService.AddUserToGroupAsync(request.UserName, request.Email, request.GroupName);

        _groupLocations.TryAdd(request.UserName, new UserLocation());

        var userInGroup = group.Users.FirstOrDefault(u => EF.Functions.ILike(u.UserName, request.UserName));
        if (userInGroup is null)
        {
            return NotFound("User not found in the group.");
        }

        _context.Users.Add(userInGroup);
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();

        return Ok(userInGroup);
    }

    /// <summary>
    /// Removes a user from a group.
    /// If the user does not exist in the group, it returns a NotFound result.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    [HttpDelete("delete/username/{userName}/groupname/{groupName}")]
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
    /// Removes a user from the system.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    [HttpDelete("delete/username/{userName}")]
    public async Task<IActionResult> RemoveUserFromSystem(string? userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest("User name is required.");
            }
            // Check if the user exists in any group
            var userGroups = await _groupService.GetUserGroupsAsync(userName)!;
            if (userGroups == null || !userGroups.Any())
            {
                return NotFound(new { Message = "User not found in any group." });
            }
            // Remove the user from all groups
            foreach (var userGroup in userGroups)
            {
                await _groupService.RemoveUserFromGroupAsync(userName, userGroup.Name);
            }
    
            return Ok(new { Message = "User removed successfully." });
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
    [HttpPatch("update/username/{userName}/oldgroupname/{groupName}/newgroupname/{newGroupName}")]
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