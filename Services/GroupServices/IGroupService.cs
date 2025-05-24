namespace GroupGeoCheckIn.Services.GroupServices;

using GeoCheckInBackend.Models;
using Microsoft.AspNetCore.Mvc;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(string groupName);
}