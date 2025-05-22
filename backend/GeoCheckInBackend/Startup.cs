namespace GeoCheckInBackend;
using GeoCheckInBackend.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // This enables MVC controller support
        services.AddControllers();

        // Add other services like DbContext, Authentication, etc. here later
        global::System.Object value = services.AddDbContext<CheckInContext>(options =>
        options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
