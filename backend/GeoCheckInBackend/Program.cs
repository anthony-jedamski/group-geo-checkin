/*
Developer: Anthony Jedamski
Project: GeoCheckInBackend
Description: GeoCheckInBackend - A backend service for managing check-ins and groups.
*/

using System.Text.Json;
using GeoCheckInBackend.Data;
using GeoCheckInBackend.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GeoCheckInBackend;
public class Program(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public static void Main()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true).Build();
        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = configurationRoot.GetValue<string>("DB_HOST"),
            Port = configurationRoot.GetValue<int>("DB_PORT"),
            Username = configurationRoot.GetValue<string>("DB_USER"),
            Password = configurationRoot.GetValue<string>("DB_PASSWORD"),
            Database = configurationRoot.GetValue<string>("DB_NAME"),
            SslMode = SslMode.Require
        }.ConnectionString;
        // This enables MVC controller support
        var hostBuilder = CreateHostBuilder(connectionString);
        hostBuilder.Build().Run();

    }

    public static IHostBuilder CreateHostBuilder(string connectionString) =>
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    // This enables MVC controller support
                    services.AddControllers();
                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen();
                    services.AddDbContext<CheckInContext>(options => options.UseNpgsql(connectionString));
                    services.AddScoped<IGroupService, GroupService>();
                    services.AddScoped<IUserService, UserService>();

                }).Configure((context, app) =>
                {
                    var env = context.HostingEnvironment;
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
                    app.UseExceptionHandler(config =>
                    {
                        config.Run(async context =>
                        {
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "application/json";

                            var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                            if (exception != null)
                            {
                                var result = JsonSerializer.Serialize(new { Message = "An unexpected error occurred.", Detail = exception.Message });
                                await context.Response.WriteAsync(result);
                            }
                        });
                    });
                });
            });
}
