using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Aero.EfCore;

/// <summary>
/// Represents a class for ApiAuthContextFactory.
/// </summary>
public class ApiAuthContextFactory : IDesignTimeDbContextFactory<AeroDbContext>
{
        /// <summary>
    /// CreateDbContext method.
    /// </summary>
public AeroDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            //.AddCommandLine()
            .Build();
        var connString = config.GetConnectionString("aero");
        var builder = new DbContextOptionsBuilder<AeroDbContext>();
        builder.UseNpgsql(connString, b
            => b.MigrationsAssembly(typeof(AeroDbContext).Assembly.FullName));

        return new AeroDbContext(builder.Options);
    }
}