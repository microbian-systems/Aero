namespace Aero.Services.Features;

/// <summary>
/// Represents a class for FeatureStoreBase.
/// </summary>
public abstract class FeatureStoreBase(ILogger<FeatureStoreBase> log) : IFeatureStore
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<FeatureStoreBase> log = log;

        /// <summary>
    /// GetFeature method.
    /// </summary>
public virtual Features GetFeature(string value) => GetFeatureAsync(value).GetAwaiter().GetResult();
        
        /// <summary>
    /// GetFeatureAsync method.
    /// </summary>
public abstract Task<Features> GetFeatureAsync(string value);
        
        /// <summary>
    /// GetAllFeatures method.
    /// </summary>
public virtual List<Features> GetAllFeatures() => GetAllFeaturesAsync().GetAwaiter().GetResult();

        /// <summary>
    /// GetAllFeaturesAsync method.
    /// </summary>
public abstract Task<List<Features>> GetAllFeaturesAsync();
        
    // public virtual void SetFeature(Features value) => SetFeatureAsync(value).GetAwaiter().GetResult();
    //
    // public abstract Task SetFeatureAsync(Features value);

        /// <summary>
    /// SetFeatures method.
    /// </summary>
public virtual void SetFeatures(Features value) => SetFeaturesAsync(value).GetAwaiter().GetResult();

        /// <summary>
    /// SetFeaturesAsync method.
    /// </summary>
public abstract Task SetFeaturesAsync(Features value);
        /// <summary>
    /// DeleteFeature method.
    /// </summary>
public void DeleteFeature(string feature) => DeleteFeatureAsync(feature).GetAwaiter().GetResult();
        
        /// <summary>
    /// DeleteFeatureAsync method.
    /// </summary>
public abstract Task DeleteFeatureAsync(string feature);

        /// <summary>
    /// DeleteFeatures method.
    /// </summary>
public void DeleteFeatures() => DeleteFeaturesAsync().GetAwaiter().GetResult();

        /// <summary>
    /// DeleteFeaturesAsync method.
    /// </summary>
public abstract Task DeleteFeaturesAsync();

        /// <summary>
    /// SetFeature method.
    /// </summary>
public virtual void SetFeature(Features value) => SetFeatureAsync(value).GetAwaiter().GetResult();
        
        /// <summary>
    /// SetFeatureAsync method.
    /// </summary>
public abstract Task SetFeatureAsync(Features value);
}