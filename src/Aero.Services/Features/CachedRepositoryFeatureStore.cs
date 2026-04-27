using Aero.Caching.Decorators;
using Aero.Core;

namespace Aero.Services.Features;

public class CachedRepositoryFeatureStore(
    ICachingRepositoryDecorator<Features> db,
    AppSettings settings,
    ILogger<RepositoryFeaturesStore> log)
    : RepositoryFeaturesStore(db, settings, log);