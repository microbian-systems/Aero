using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Aero.EfCore;

public class ApiAuthContextFactory : IDesignTimeDbContextFactory<AeroApiContext>
{
    public AeroApiContext CreateDbContext(string[] args)
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
        var builder = new DbContextOptionsBuilder<AeroApiContext>();
        builder.UseNpgsql(connString, b
            => b.MigrationsAssembly(typeof(AeroApiContext).Assembly.FullName));

        return new AeroApiContext(builder.Options);
    }
}