/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/

namespace GeoCheckInBackend.Services;

using GeoCheckInBackend.Models;
using GeoCheckInBackend.Data;
using Microsoft.EntityFrameworkCore;

public class GroupService : IGroupService
{
    private readonly CheckInContext _context;

    public GroupService(CheckInContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new group with the specified name.
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Group> CreateGroupAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("Group name is required.");

        var group = await _context.Groups.FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, groupName));
        if (group is not null)
        {
            return group; // Group already exists
        }
        var allGroups = await _context.Groups.OrderByDescending(g => g.Id).ToListAsync();
        int newGroupId = 0;
        if (allGroups is not null && allGroups.First().Id > 0)
        {
            newGroupId = allGroups.First().Id + 1;
            if (allGroups.Any(g => g.Id == newGroupId))
            {
                throw new InvalidOperationException("Group ID already exists. Please try again.");
            }
        }
        else
        {
            // If no groups exist, start with ID 1
            newGroupId = 1;
        }

        var newGroup = new Group
        {
            Name = groupName,
            Id = newGroupId,
            Users = new List<User>()
        };

        _context.Groups.Add(newGroup);
        await _context.SaveChangesAsync();
        return newGroup;
    }

    /// <summary>
    /// Adds a user to a group. If the group does not exist, it creates a new group with the specified name.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Group> AddUserToGroupAsync(string userName, string? email = null, string? groupName = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.");

        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, groupName!));

        if (group == null)
        {
            group = await CreateGroupAsync(groupName ?? "Default Group");
        }

        if (group.Users.Any(u => u.UserName.ToLower() == userName.ToLower()))
            return group; // User already exists in the group


        var emailName = email ?? string.Concat(userName, new Random().Next(0, 1000).ToString(), "@example.com");

        var user = new User
        {
            UserName = userName,
            GroupId = group.Id,
            Email = emailName,
        };

        group.Users.Add(user);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Removes a user from a group. If the user does not exist in the group, it throws an exception.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Group> RemoveUserFromGroupAsync(string userName, string groupName)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("User name and group name are required.");

        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, groupName));

        if (group == null)
            throw new InvalidOperationException("Group not found.");

        var user = group.Users.FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());
        if (user == null)
            throw new InvalidOperationException("User not found in the group.");

        group.Users.Remove(user);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Gets all groups that a user belongs to.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<List<Group>>? GetUserGroupsAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.");

        var groups = await _context.Groups
            .Where(g => g.Users.Any(u => EF.Functions.ILike(u.UserName, userName)))
            .ToListAsync();

        return groups;
    }

    public async Task<Group?> GetGroupByNameAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return null;

        return await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, groupName));
    }
}
