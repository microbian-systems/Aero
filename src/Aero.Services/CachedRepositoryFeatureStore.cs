using Aero.Caching.Decorators;

namespace Aero.Services;

public class CachedRepositoryFeatureStore(
    ICachingRepositoryDecorator<Features.Features> db,
    AppSettings settings,
    ILogger<RepositoryFeaturesStore> log)
    : RepositoryFeaturesStore(db, settings, log);