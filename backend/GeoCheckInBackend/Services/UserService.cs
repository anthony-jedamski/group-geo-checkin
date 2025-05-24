/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/

namespace GeoCheckInBackend.Services;

using GeoCheckInBackend.Models;
using GeoCheckInBackend.Data;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly CheckInContext _context;

    public UserService(CheckInContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserAsync(string? userName, int? groupId)
    {
        if (string.IsNullOrEmpty(userName) || groupId <= 0)
        {
            return null;
        }

        var user = await _context.Users
                   .FirstOrDefaultAsync(u =>
                       EF.Functions.ILike(u.UserName, userName) &&
                       u.GroupId == groupId); // Adjust as necessary

        return user;
    }

    public async Task<User?> GetUserAsync(string? userName, string? groupName)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(groupName))
        {
            return null;
        }

        var group = await _context.Groups
            .FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, groupName));

        if (group == null)
        {
            return null; // Group not found
        }

        var user = await _context.Users
                   .FirstOrDefaultAsync(u =>
                       EF.Functions.ILike(u.UserName, userName) &&
                       u.GroupId == group.Id);

        return user;
    }

    public async Task<List<User>> GetUsersAsync(string? userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return new List<User>();
        }

        var users = new List<User>();
        users = await _context.Users
          .Where(u => EF.Functions.ILike(u.UserName, userName))
          .ToListAsync();
           return users;
    }
}