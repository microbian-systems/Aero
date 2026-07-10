using Aero.Core.Extensions;


namespace Aero.Services.Features;

/// <summary>
/// Represents a class for FeatureServiceBase.
/// </summary>
public abstract class FeatureServiceBase(IFeatureStore store, ILogger<FeatureServiceBase> log) : IFeaturesService
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<FeatureServiceBase> log = log;
        /// <summary>
    /// store.
    /// </summary>
protected readonly IFeatureStore store = store;

        /// <summary>
    /// GetFeature method.
    /// </summary>
public Features GetFeature(string feature) => GetFeatureAsync(feature).GetAwaiter().GetResult();

        /// <summary>
    /// GetFeatureAsync method.
    /// </summary>
public async Task<Features> GetFeatureAsync(string feature)
    {
        log.LogInformation($"getting feaeture {feature}");
        return await store.GetFeatureAsync(feature);
    }

        /// <summary>
    /// GetAllFeatures method.
    /// </summary>
public List<Features> GetAllFeatures() => GetAllFeaturesAsync().GetAwaiter().GetResult();

        /// <summary>
    /// GetAllFeaturesAsync method.
    /// </summary>
public async Task<List<Features>> GetAllFeaturesAsync()
    {
        log.LogInformation($"getting all features");
        return await store.GetAllFeaturesAsync();
    }

        /// <summary>
    /// SetFeature method.
    /// </summary>
public void SetFeature(Features feature) => SetFeatureAsync(feature).GetAwaiter().GetResult();
        
        /// <summary>
    /// SetFeatureAsync method.
    /// </summary>
public async Task SetFeatureAsync(Features feature)
    {
        log.LogInformation($"setting feature {feature.ToJson()}");
        await store.SetFeatureAsync(feature);
    }

        /// <summary>
    /// SetFeatures method.
    /// </summary>
public void SetFeatures(Features features) => SetFeaturesAsync(features).GetAwaiter().GetResult();

        /// <summary>
    /// SetFeaturesAsync method.
    /// </summary>
public async Task SetFeaturesAsync(Features features)
    {
        log.LogInformation($"setting all features {features.ToJson()}");
        await store.SetFeaturesAsync(features);
    }

        /// <summary>
    /// DeleteFeature method.
    /// </summary>
public void DeleteFeature(string feature) => DeleteFeatureAsync(feature).GetAwaiter().GetResult();

        /// <summary>
    /// DeleteFeatureAsync method.
    /// </summary>
public async Task DeleteFeatureAsync(string feature)
    {
        log.LogInformation($"deleting feature {feature}");
        await store.DeleteFeatureAsync(feature);
    }

        /// <summary>
    /// DeleteAllFeatures method.
    /// </summary>
public void DeleteAllFeatures() => DeleteAllFeaturesAsync().GetAwaiter().GetResult();

        /// <summary>
    /// DeleteAllFeaturesAsync method.
    /// </summary>
public async Task DeleteAllFeaturesAsync()
    {
        log.LogWarning($"*** warning **** - deleting all features");
        await store.DeleteFeaturesAsync();
    }
}