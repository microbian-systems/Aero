using Scalar.AspNetCore;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for ScalarUIExtensions.
/// </summary>
public static class ScalarUIExtensions
{
        /// <summary>
    /// AddScalarUI method.
    /// </summary>
public static WebApplication AddScalarUI(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            app.MapScalarApiReference(options =>
            {
                options
                    .WithPreferredScheme("Bearer")
                    .WithHttpBearerAuthentication(new HttpBearerOptions())
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithDownloadButton(true)
                    .WithClientButton(true);
            });
        }
        
        return app;
    }
}