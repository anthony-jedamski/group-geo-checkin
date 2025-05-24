namespace GroupGeoCheckIn.Services;

using GeoCheckInBackend.Models;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(string groupName);
}