namespace GroupGeoCheckIn.Services;

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

    public async Task<Group> CreateGroupAsync(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("Group name is required.");

        var allGroups = await _context.Groups.ToListAsync();
        if (allGroups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Group with this name already exists.");

        var groupId = allGroups.Any() ? allGroups.Max(g => g.Id) + 1 : 1;

        var group = new Group
        {
            Name = groupName,
            Id = groupId,
            Users = new List<User>()
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }
}
