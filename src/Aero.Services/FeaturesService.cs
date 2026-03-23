namespace Aero.Services;

public sealed class FeaturesService(IFeatureStore store, ILogger<FeaturesService> log) : FeatureServiceBase(store, log);