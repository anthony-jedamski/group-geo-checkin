/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/
namespace GeoCheckInBackend.Services;

using GeoCheckInBackend.Models;
public interface IUserService
{
    Task<User?> GetUserAsync(string? userName, int? groupId);
    Task<User?> GetUserAsync(string? userName, string? groupName);
}