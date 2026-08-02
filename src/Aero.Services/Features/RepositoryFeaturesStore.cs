using Aero.Core;
using Aero.Core.Data;
using Aero.Core.Extensions;



namespace Aero.Services.Features;


// todo - finish implementing feature store
/// <summary>
/// Represents a class for RepositoryFeaturesStore.
/// </summary>
public class RepositoryFeaturesStore(
    IGenericRepository<Features> repo,
    AppSettings settings,
    ILogger<RepositoryFeaturesStore> log)
    : FeatureStoreBase(log)
{
    private readonly string appName;
    private readonly IGenericRepository<Features> repo = repo;
    private readonly AppSettings settings = settings;

        /// <summary>
    /// GetFeatureAsync method.
    /// </summary>
public override async Task<Features> GetFeatureAsync(string value)
    {
        log.LogInformation($"getting feature: {value}");
        var result = await GetAllFeaturesAsync();
        var feature = result.First(x => string.Equals(x.Feature,
            value, StringComparison.InvariantCultureIgnoreCase));

        return feature;
    }

        /// <summary>
    /// GetAllFeaturesAsync method.
    /// </summary>
public override async Task<List<Features>> GetAllFeaturesAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        // log.LogInformation($"getting all features for {AppSettings.AppName}");
        // return await repo.GetAllAsync();
    }

        /// <summary>
    /// SetFeaturesAsync method.
    /// </summary>
public override async Task SetFeaturesAsync(Features value)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// SetFeatureAsync method.
    /// </summary>
public override async Task SetFeatureAsync(Features value)
    {
        await Task.CompletedTask;
        log.LogInformation($"setting feature for: {value.ToJson()}");
        throw new NotImplementedException();
        // var features = await GetAllFeaturesAsync();
        //
        // var index = features.FindIndex(x =>
        //     string.Equals(value.Feature, x.Feature, StringComparison.InvariantCultureIgnoreCase));
        //
        // if (index >= 0)
        // {
        //     features[index.Value] = value;
        // }
        // else
        // {
        //     features.Add(value);
        // }
        //
        // await repo.UpsertAsync((Features) features);
    }


        /// <summary>
    /// DeleteFeatureAsync method.
    /// </summary>
public override async Task DeleteFeatureAsync(string feature)
    {
        await Task.CompletedTask;
        log.LogInformation($"deleting feature {feature}");

        throw new NotImplementedException();
        // var features = await GetAllFeaturesAsync();
        // var item = features?.Featuress?.First(x => 
        //     string.Equals(x.Feature, feature, StringComparison.InvariantCultureIgnoreCase));
        // features?.Featuress?.Remove(item);
        //
        // if (features == null)
        // {
        //     log.LogInformation($"unable to find feature {feature}");
        //     return;
        // }
        //
        // await repo.UpsertAsync((Features) features);
    }

        /// <summary>
    /// DeleteFeaturesAsync method.
    /// </summary>
public override async Task DeleteFeaturesAsync()
    {
        await Task.CompletedTask;
        log.LogWarning($"deleting all features ");

        throw new NotImplementedException();
        // var features = await GetAllFeaturesAsync();
        // await repo.DeleteAsync((Features) features);
    }
}