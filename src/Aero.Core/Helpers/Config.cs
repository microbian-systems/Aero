using System.Text.RegularExpressions;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using ILogger = Serilog.ILogger;

namespace Aero.Core.Helpers;

/// <summary>
/// Represents a class for Config.
/// </summary>
public static class Config
{
        /// <summary>
    /// GetSetting method.
    /// </summary>
public static string GetSetting(string key) => ConfigurationManager.AppSettings[key];

        /// <summary>
    /// GetConnString method.
    /// </summary>
public static string GetConnString(string key) =>
        ConfigurationManager.ConnectionStrings[key]?.ConnectionString;

        /// <summary>
    /// GetStorageConnectionString method.
    /// </summary>
public static string GetStorageConnectionString()
    {
        return GetSetting("blobStorage");
    }

        /// <summary>
    /// GetJobCommand method.
    /// </summary>
public static T GetJobCommand<T>(ILogger log = null) //where T : IJobModel
    {
        var config = GetSetting("job"); // todo - check for null on config and do something...
        log?.Information($"config: {config}");
        if (string.IsNullOrEmpty(config))
            return default(T);
        var json = Regex.Unescape(config);
        var model = JsonSerializer.Deserialize<T>(json);

        return model;
    }

    // todo - wire method up to use the Azure KeyVault client
        /// <summary>
    /// GetFromAzureVault method.
    /// </summary>
public static string GetFromAzureVault(string key) => GetSetting(key);
}