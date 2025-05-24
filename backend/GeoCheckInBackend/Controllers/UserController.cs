namespace GeoCheckInBackend.Controllers;

using GeoCheckInBackend.Data;
using GeoCheckInBackend.Models;
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
    /*
        | Method | URL                                     | Description                |
    | ------ | --------------------------------------- | -------------------------- |
    | POST   | `/api/GroupCheckIn/register`            | Register a user            |
    | POST   | `/api/GroupCheckIn/group`               | Create a group             |
    | POST   | `/api/GroupCheckIn/checkin`             | Submit a location check-in |
    | GET    | `/api/GroupCheckIn/checkins/group/{id}` | Get group check-ins        |
    */
    private static readonly ConcurrentDictionary<string, UserLocation> _groupLocations = new();
    private readonly CheckInContext _context;

    public CheckInController(CheckInContext context)
    {
        _context = context;
    }


    // POST: api/GroupCheckIn/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] User user)
    {
        bool createGroup = false;
        var allGroups = await _context.Groups.ToListAsync();
        //If user group is not included in the request.
        if (user.Group is null)
        {
            if (allGroups.Any(g => g.Name.Equals(user.Group?.Name, StringComparison.OrdinalIgnoreCase)))
            {
                user.Group = allGroups.First(g => g.Name.Equals(user.Group?.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                createGroup = true;
            }
        }
        else
        {
            if (user.Group.Name is not null && allGroups.Any(g => g.Name.Equals(user.Group.Name, StringComparison.OrdinalIgnoreCase)))
            {
                user.Group = allGroups.First(g => g.Name.Equals(user.Group.Name, StringComparison.OrdinalIgnoreCase));
            }
            else if(user.Group.Id > 0 && allGroups.Any(g => g.Id == user.Group.Id))
            {
                user.Group = allGroups.First(g => g.Id == user.Group.Id);
            }
            else
            {
                createGroup = true;
            }
        }
        if (createGroup)//todo: finish creating group.
        {
            if (string.IsNullOrWhiteSpace(user.Group?.Name))
                return BadRequest("Group name is required.");
            var groupId = allGroups.Any() ? allGroups.Max(g => g.Id) + 1 : 1;
            user.Group.Id = groupId;
            _context.Groups.Add(user.Group);
            allGroups = await _context.Groups.ToListAsync();
            if (allGroups.Any(g => g.Name.Equals(user.Group.Name, StringComparison.OrdinalIgnoreCase)))
            {
                user.Group = allGroups.First(g => g.Name.Equals(user.Group.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return BadRequest("Group creation failed.");
            }
        }
   

        var allUsers = await _context.Users.ToListAsync();
        if (allUsers.Any(u => u.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Ok("User with this name already exists.");
        }
        

        //Handle the case where the user is not in a group.
        int? userId;
        if(user.Id == 0)
        {
            userId = allUsers.Any() ? allUsers.Max(u => u.Id) + 1 : 1;
        }
        else
        {
            userId = user.Id;
        }
        //Check to see if the user is already in the group.
        if (user.Group?.Users != null && user.Group.Users.Any(u => u.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Ok("User with this name already exists in the group.");
        }

        //If the user is not in the group, add them to the group.
        user.Group?.Users.Add(user);
        _groupLocations.TryAdd(user.Name, new UserLocation());
        _context.Users.Add(user);
        _context.Groups.Update(user.Group!);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

/*     // POST: api/GroupCheckIn/group
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] Group group)
    {
        if (string.IsNullOrWhiteSpace(group.Name))
            return BadRequest("Group name is required.");

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return Ok(group);
    }
 */

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] LocationCheckIn checkIn)
    {
        checkIn.Timestamp = DateTime.UtcNow;
        _context.LocationCheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        return Ok(checkIn);
    }

    
}