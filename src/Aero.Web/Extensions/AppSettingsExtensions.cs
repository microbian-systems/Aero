using System.Reflection;

namespace Aero.Web.Extensions;

public static class AppSettingsExtensions
{
    public static WebApplicationBuilder AddAppSettings<T>(this WebApplicationBuilder builder)
        where T : class
    {
        var env = builder.Environment;

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (env.IsDevelopment())
        {
            // allegedly aot compatible
            builder.Configuration.AddUserSecrets<T>(optional: true);
        }

        return builder;
    }
}