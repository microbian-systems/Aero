using Aero.Core.Encryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aero.Core;

/// <summary>
/// Represents a class for EncryptionExtensions.
/// </summary>
public static class EncryptionExtensions
{
        /// <summary>
    /// AddEncryptionServices method.
    /// </summary>
public static IServiceCollection AddEncryptionServices(this IServiceCollection services)
    {
        services.AddTransient<IEncryptor, Aes256Encryptor>(sp =>
        {
            var monitor = sp.GetRequiredService<IOptionsMonitor<AppSettings>>();
            var settings = monitor.CurrentValue;
            var key = settings.AesEncryptionSettings.Key;
            var iv = settings.AesEncryptionSettings.IV;
            var opts = new AesEncryptorOptions(key, iv);
            var encryptor = new Aes256Encryptor(opts);

            return encryptor;
        });
        return services;
    }
}