namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for AppSettingsExtensions.
/// </summary>
public static class AppSettingsExtensions
{
        /// <summary>
    /// AddAppSettings method.
    /// </summary>
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