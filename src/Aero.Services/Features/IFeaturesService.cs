namespace Aero.Services.Features;

/// <summary>
/// Defines an interface for IFeaturesService.
/// </summary>
public interface IFeaturesService
{
        /// <summary>
    /// GetFeature method.
    /// </summary>
Features GetFeature(string feature);
        /// <summary>
    /// GetFeatureAsync method.
    /// </summary>
Task<Features> GetFeatureAsync(string feature);
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
void SetFeature(Features feature);
        /// <summary>
    /// SetFeatureAsync method.
    /// </summary>
Task SetFeatureAsync(Features feature);
        /// <summary>
    /// SetFeatures method.
    /// </summary>
void SetFeatures(Features features);
        /// <summary>
    /// SetFeaturesAsync method.
    /// </summary>
Task SetFeaturesAsync(Features features);
        /// <summary>
    /// DeleteFeature method.
    /// </summary>
void DeleteFeature(string feature);
        /// <summary>
    /// DeleteFeatureAsync method.
    /// </summary>
Task DeleteFeatureAsync(string feature);
        /// <summary>
    /// DeleteAllFeatures method.
    /// </summary>
void DeleteAllFeatures();
        /// <summary>
    /// DeleteAllFeaturesAsync method.
    /// </summary>
Task DeleteAllFeaturesAsync();
}