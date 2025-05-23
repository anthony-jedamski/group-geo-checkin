namespace GeoCheckInBackend;

using GeoCheckInBackend.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // This enables MVC controller support
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Add other services like DbContext, Authentication, etc. here later
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("DB_HOST"),
            Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT")!),
            Username = Environment.GetEnvironmentVariable("DB_USER"),
            Password = Environment.GetEnvironmentVariable("DB_PASSWORD"),
            Database = Environment.GetEnvironmentVariable("DB_NAME"),
            SslMode = SslMode.Require
        };
        services.AddDbContext<CheckInContext>(options =>
            options.UseNpgsql(builder.ConnectionString));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
         if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeoCheckIn API V1");
            });
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
