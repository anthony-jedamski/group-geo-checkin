/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/

namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
using GeoCheckInBackend.Models.Requests;
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
    private readonly IUserService _userService;
    public UserController(CheckInContext context, IGroupService groupService, IUserService userService)
    {
        _context = context;
        _groupService = groupService;
        _userService = userService;
    }

    [HttpGet("get/username/{userName}")]
    public async Task<IActionResult> GetUser(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return BadRequest(new { Message = "User name is required." });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName, userName)); // For PostgreSQL

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        return Ok(user);
    }

    /// <summary>
    /// Retrieves all users from the system.
    /// If no users are found, it returns a NotFound result with a message.
    /// </summary>
    /// <returns></returns>
    [HttpGet("get/all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null || !users.Any())
        {
            return NotFound(new { Message = "No users found." });
        }
        return Ok(users);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { Message = "Username and email are required." });
        }

        var user = request.GroupId is not null ?
            await _userService.GetUserAsync(request.UserName, request.GroupId) :
            await _userService.GetUserAsync(request.UserName, request.GroupName);
        if (user != null)
        {
            return Ok(new { Message = "User with this username or email already exists." });
        }

        var group = await _groupService.AddUserToGroupAsync(request.UserName, request.Email, request.GroupName);

        _groupLocations.TryAdd(request.UserName, new UserLocation());

        var userInGroup = group.Users.FirstOrDefault(u => request.UserName.ToLower() == u.UserName.ToLower());
        if (userInGroup is null)
        {
            return NotFound(new { Message = "User not found in the group." });
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
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
        {
            return BadRequest(new { Message = "User name and group name are required." });
        }
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
            return BadRequest(new { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { ex.Message });
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
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest(new { Message = "User name is required." });

        try
        {

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
            return BadRequest(new { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { ex.Message });
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
    [HttpPatch("updateUserGroup")]
    public async Task<IActionResult> UpdateUserGroup([FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.OldGroupName) || string.IsNullOrWhiteSpace(request.NewGroupName))
        {
            return BadRequest(new { Message = "User name, group name, and new group name are required." });
        }

        try
        {
            await _groupService.RemoveUserFromGroupAsync(request.UserName, request.OldGroupName);
            var group = await _groupService.AddUserToGroupAsync(request.UserName, request.NewGroupName);
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new {ex.Message });
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
            return BadRequest(new { Message = "User name, group name, and new group name are required." });
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