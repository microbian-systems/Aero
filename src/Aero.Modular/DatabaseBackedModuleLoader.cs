namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;
using Microsoft.Extensions.Logging;

/// <summary>
/// Loads module state from the database and merges it with reflection-discovered module types.
/// This enables subsequent application runs to skip expensive reflection-based discovery
/// while preserving user modifications (Disabled, Order, Description, etc.) from the database.
/// </summary>
public sealed class DatabaseBackedModuleLoader(
    IModuleStateStore stateStore,
    IModuleDiscoveryService discoveryService,
    ILogger<DatabaseBackedModuleLoader> logger)
    : IModuleStateLoader
{
    /// <inheritdoc/>
    public async Task<bool> HasStoredModulesAsync(CancellationToken ct = default)
    {
        var states = await stateStore.GetAllAsync(ct);
        return states.Count > 0;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModuleStateDocument>> LoadModuleStatesAsync(CancellationToken ct = default)
    {
        return await stateStore.GetAllAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModuleDescriptor>> LoadMergedModulesAsync(CancellationToken ct = default)
    {
        // First, discover modules via reflection to get ModuleType and other runtime properties
        var discoveredDescriptors = await discoveryService.DiscoverAsync(ct);
        
        // Load persisted state from database
        var storedStates = await stateStore.GetAllAsync(ct);
        
        if (storedStates.Count == 0)
        {
            logger.LogDebug("No stored module state found, returning reflection-discovered modules");
            return discoveredDescriptors;
        }

        // Build a lookup of stored state by module name
        var storedStatesByName = storedStates.ToDictionary(
            s => s.Name,
            s => s,
            StringComparer.OrdinalIgnoreCase);

        // Merge: use reflection for ModuleType and runtime properties, DB for persisted modifications
        var mergedDescriptors = new List<ModuleDescriptor>();

        foreach (var descriptor in discoveredDescriptors)
        {
            if (storedStatesByName.TryGetValue(descriptor.Name, out var storedState))
            {
                var merged = MergeDescriptor(descriptor, storedState);
                mergedDescriptors.Add(merged);
                
                logger.LogDebug(
                    "Merged stored state for module '{ModuleName}': Disabled={Disabled}, Order={Order}",
                    descriptor.Name, merged.Disabled, merged.Order);
            }
            else
            {
                // Module discovered via reflection but not in DB - this is a new module
                // Return as-is (it will be saved to DB on next SetupCompletionService run)
                mergedDescriptors.Add(descriptor);
                
                logger.LogDebug(
                    "Module '{ModuleName}' discovered via reflection but not in DB - using reflection state",
                    descriptor.Name);
            }
        }

        logger.LogInformation(
            "Loaded {Count} modules from DB state merged with {Total} discovered modules",
            mergedDescriptors.Count,
            discoveredDescriptors.Count);

        return mergedDescriptors;
    }

    private static ModuleDescriptor MergeDescriptor(ModuleDescriptor reflection, ModuleStateDocument stored)
    {
        return new ModuleDescriptor
        {
            Name = reflection.Name,
            Version = reflection.Version,
            Author = reflection.Author,
            ModuleType = reflection.ModuleType,
            Dependencies = stored.Dependencies.Count > 0 
                ? stored.Dependencies 
                : reflection.Dependencies,
            AssemblyName = reflection.AssemblyName,
            PhysicalPath = reflection.PhysicalPath,
            IsUiModule = reflection.IsUiModule,
            Order = stored.Order,
            Category = stored.Category.Count > 0 
                ? stored.Category 
                : reflection.Category,
            Tags = stored.Tags.Count > 0 
                ? stored.Tags 
                : reflection.Tags,
            DisabledInProduction = stored.DisabledInProduction,
            Disabled = stored.Disabled
        };
    }
}
