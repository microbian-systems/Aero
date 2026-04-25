using Aero.Caching.Decorators;

namespace Aero.Services.Features;

public class CachedRepositoryFeatureStore(
    ICachingRepositoryDecorator<Features> db,
    AppSettings settings,
    ILogger<RepositoryFeaturesStore> log)
    : RepositoryFeaturesStore(db, settings, log);