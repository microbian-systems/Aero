namespace Aero.Services;

/// <summary>
/// Represents a class for FeaturesService.
/// </summary>
public sealed class FeaturesService(IFeatureStore store, ILogger<FeaturesService> log) : FeatureServiceBase(store, log);