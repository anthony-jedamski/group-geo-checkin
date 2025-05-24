/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/
namespace GeoCheckInBackend.Services;

using GeoCheckInBackend.Models;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(string groupName);
    Task<Group> AddUserToGroupAsync(string userName, string? email = null, string? groupName = null);
    Task<Group> RemoveUserFromGroupAsync(string userName, string groupName);
    Task<List<Group>>? GetUserGroupsAsync(string userName);
    Task<Group?> GetGroupByNameAsync(string groupName);
}