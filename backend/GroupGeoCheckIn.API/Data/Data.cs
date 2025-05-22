// First, install the PostgreSQL EF Core provider:
// In your terminal, run:
// dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

using Microsoft.EntityFrameworkCore;

namespace GroupGeoCheckIn.Data
{
    public class CheckInContext : DbContext
    {
        public CheckInContext(DbContextOptions<CheckInContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<LocationCheckIn> LocationCheckIns { get; set; } = null!;
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
    }

    public class LocationCheckIn
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
