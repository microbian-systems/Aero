namespace Aero.Services.Features;

/// <summary>
/// Defines an interface for IFeatureStore.
/// </summary>
public interface IFeatureStore
{
        /// <summary>
    /// GetFeature method.
    /// </summary>
Features GetFeature(string value);
        /// <summary>
    /// GetFeatureAsync method.
    /// </summary>
Task<Features> GetFeatureAsync(string value);
        /// <summary>
    /// GetAllFeatures method.
    /// </summary>
List<Features> GetAllFeatures();
        /// <summary>
    /// GetAllFeaturesAsync method.
    /// </summary>
Task<List<Features>> GetAllFeaturesAsync();
        /// <summary>
    /// SetFeature method.
    /// </summary>
void SetFeature(Features value);
        /// <summary>
    /// SetFeatureAsync method.
    /// </summary>
Task SetFeatureAsync(Features value);
        /// <summary>
    /// SetFeatures method.
    /// </summary>
void SetFeatures(Features value);
        /// <summary>
    /// SetFeaturesAsync method.
    /// </summary>
Task SetFeaturesAsync(Features value);
        /// <summary>
    /// DeleteFeature method.
    /// </summary>
void DeleteFeature(string feature);
        /// <summary>
    /// DeleteFeatureAsync method.
    /// </summary>
Task DeleteFeatureAsync(string feature);
        /// <summary>
    /// DeleteFeatures method.
    /// </summary>
void DeleteFeatures();
        /// <summary>
    /// DeleteFeaturesAsync method.
    /// </summary>
Task DeleteFeaturesAsync();
}