using Aero.Social.Forem;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.Social.Extensions;

/// <summary>
/// Represents a class for SocialExtensions.
/// </summary>
public static class SocialExtensions
{
        /// <summary>
    /// AddAeroSocials method.
    /// </summary>
public static IServiceCollection AddAeroSocials(this IServiceCollection services)
    {
        services.AddForem();
        return services;
    }
}