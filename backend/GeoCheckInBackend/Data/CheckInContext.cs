namespace GeoCheckInBackend.Data;

using GeoCheckInBackend.Models;
using Microsoft.EntityFrameworkCore;

public class CheckInContext : DbContext
{
    public CheckInContext(DbContextOptions<CheckInContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<LocationCheckIn> LocationCheckIns { get; set; } = null!;
}


