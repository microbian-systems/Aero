namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Default implementation of module discovery using Scrutor-compatible reflection patterns.
/// Supports optional database-backed state loading for subsequent runs.
/// </summary>
public sealed class ModuleDiscoveryService(
    IOptions<ModuleDiscoveryOptions> options,
    IHostEnvironment environment,
    ILogger<ModuleDiscoveryService> logger,
    IModuleStateStore? stateStore = null)
    : IModuleDiscoveryService
{
    private readonly ModuleDiscoveryOptions _options = options.Value;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModuleDescriptor>> DiscoverAsync(CancellationToken ct = default)
    {
        // If state store is available and has stored modules, load from DB
        if (stateStore != null)
        {
            var storedStates = await stateStore.GetAllAsync(ct);
            if (storedStates.Count > 0)
            {
                logger.LogDebug("Found {Count} stored modules in database, loading via reflection to resolve types", storedStates.Count);
                // We still need reflection to resolve ModuleType, but we use stored state for metadata
                return await DiscoverAndMergeWithStoredStateAsync(storedStates, ct);
            }
        }

        logger.LogDebug("No stored module state found, performing reflection-based discovery");
        return await DiscoverViaReflectionAsync(ct);
    }

    private async Task<IReadOnlyList<ModuleDescriptor>> DiscoverAndMergeWithStoredStateAsync(
        IReadOnlyList<ModuleStateDocument> storedStates, 
        CancellationToken ct)
    {
        var discoveredDescriptors = await DiscoverViaReflectionAsync(ct);
        
        // Build lookup of stored state by module name
        var storedByName = storedStates.ToDictionary(
            s => s.Name,
            s => s,
            StringComparer.OrdinalIgnoreCase);

        var merged = new List<ModuleDescriptor>();
        foreach (var descriptor in discoveredDescriptors)
        {
            if (storedByName.TryGetValue(descriptor.Name, out var stored))
            {
                merged.Add(MergeWithStoredState(descriptor, stored));
                logger.LogDebug("Merged stored state for module '{ModuleName}'", descriptor.Name);
            }
            else
            {
                merged.Add(descriptor);
                logger.LogDebug("Module '{ModuleName}' not in stored state, using reflection data", descriptor.Name);
            }
        }

        logger.LogInformation("Loaded {Count} modules (merged with stored state)", merged.Count);
        return merged;
    }

    private static ModuleDescriptor MergeWithStoredState(ModuleDescriptor reflection, ModuleStateDocument stored)
    {
        return new ModuleDescriptor
        {
            Name = reflection.Name,
            Version = reflection.Version,
            Author = reflection.Author,
            ModuleType = reflection.ModuleType,
            Dependencies = stored.Dependencies.Count > 0 ? stored.Dependencies : reflection.Dependencies,
            AssemblyName = reflection.AssemblyName,
            PhysicalPath = reflection.PhysicalPath,
            IsUiModule = reflection.IsUiModule,
            Order = stored.Order,
            Category = stored.Category.Count > 0 ? stored.Category : reflection.Category,
            Tags = stored.Tags.Count > 0 ? stored.Tags : reflection.Tags,
            DisabledInProduction = stored.DisabledInProduction,
            Disabled = stored.Disabled
        };
    }

    private Task<IReadOnlyList<ModuleDescriptor>> DiscoverViaReflectionAsync(CancellationToken ct)
    {
        var descriptors = new List<ModuleDescriptor>();
        var seenNames = new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase);

        var assembliesToScan = GetAssembliesToScan();

        foreach (var assembly in assembliesToScan)
        {
            ct.ThrowIfCancellationRequested();

            IEnumerable<Type> moduleTypes;
            try
            {
                moduleTypes = ScanAssemblyForModules(assembly);
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.LogWarning(ex, "Partial load of assembly '{Assembly}' - some types may be skipped.", assembly.FullName);
                moduleTypes = ex.Types.Where(t => t != null && IsValidModuleType(t))!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to scan assembly '{Assembly}' for modules.", assembly.FullName);
                continue;
            }

            foreach (var type in moduleTypes)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var descriptor = CreateDescriptor(type);

                    // Validate unique module names
                    if (seenNames.TryGetValue(descriptor.Name, out var existing))
                    {
                        throw new DuplicateModuleNameException(
                            descriptor.Name,
                            existing.AssemblyName,
                            descriptor.AssemblyName);
                    }

                    // Skip modules disabled in production unless configured otherwise
                    if (IsDisabledInProduction(type) && !_options.IncludeDisabledInProduction && environment.IsProduction())
                    {
                        logger.LogInformation("Skipping module '{ModuleName}' - disabled in production.", descriptor.Name);
                        continue;
                    }

                    seenNames[descriptor.Name] = descriptor;
                    descriptors.Add(descriptor);

                    logger.LogDebug("Discovered module '{ModuleName}' v{Version} from '{Assembly}'.",
                        descriptor.Name, descriptor.Version, descriptor.AssemblyName);
                }
                catch (DuplicateModuleNameException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create descriptor for type '{Type}' in assembly '{Assembly}'.",
                        type.FullName, assembly.FullName);
                }
            }
        }

        logger.LogInformation("Discovered {Count} modules.", descriptors.Count);
        return Task.FromResult<IReadOnlyList<ModuleDescriptor>>(descriptors);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ModuleDescriptor>> DiscoverFromTypesAsync(IEnumerable<Type> moduleTypes, CancellationToken ct = default)
    {
        var descriptors = new List<ModuleDescriptor>();
        var seenNames = new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in moduleTypes)
        {
            ct.ThrowIfCancellationRequested();

            if (!IsValidModuleType(type))
            {
                logger.LogWarning("Type '{Type}' is not a valid module type - must be non-abstract class implementing IAeroModule.", type.FullName);
                continue;
            }

            try
            {
                var descriptor = CreateDescriptor(type);

                if (seenNames.TryGetValue(descriptor.Name, out var existing))
                {
                    throw new DuplicateModuleNameException(
                        descriptor.Name,
                        existing.AssemblyName,
                        descriptor.AssemblyName);
                }

                seenNames[descriptor.Name] = descriptor;
                descriptors.Add(descriptor);
            }
            catch (DuplicateModuleNameException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create descriptor for type '{Type}'.", type.FullName);
            }
        }

        return Task.FromResult<IReadOnlyList<ModuleDescriptor>>(descriptors);
    }

    private IEnumerable<Assembly> GetAssembliesToScan()
    {
        var assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            var assemblyName = assembly.GetName().Name;
            if (IsExcludedAssembly(assemblyName))
                continue;

            AddAssembly(assemblies, assembly);
        }

        if (_options.ScanApplicationDependencies)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var dependencyContext = entryAssembly != null
                ? DependencyContext.Load(entryAssembly) ?? DependencyContext.Default
                : DependencyContext.Default;

            if (dependencyContext != null)
            {
                foreach (var assemblyName in dependencyContext.RuntimeLibraries
                             .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
                             .Distinct())
                {
                    TryAddAssembly(assemblies, assemblyName);
                }
            }
            else if (entryAssembly != null)
            {
                AddAssembly(assemblies, entryAssembly);
                foreach (var referenced in entryAssembly.GetReferencedAssemblies())
                {
                    TryAddAssembly(assemblies, referenced);
                }
            }
        }

        // Scan additional paths
        foreach (var path in _options.AdditionalScanPaths)
        {
            if (!Directory.Exists(path))
            {
                logger.LogWarning("Module scan path does not exist: '{Path}'.", path);
                continue;
            }

            var dlls = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls)
            {
                try
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(dll);
                    if (IsExcludedAssembly(assemblyName))
                        continue;

                    var assembly = Assembly.LoadFrom(dll);
                    AddAssembly(assemblies, assembly);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not load assembly from '{Path}'.", dll);
                }
            }
        }

        return assemblies.Values;
    }

    private void TryAddAssembly(IDictionary<string, Assembly> assemblies, AssemblyName assemblyName)
    {
        if (IsExcludedAssembly(assemblyName.Name))
            return;

        try
        {
            AddAssembly(assemblies, Assembly.Load(assemblyName));
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not load referenced assembly '{Assembly}'.", assemblyName.Name);
        }
    }

    private static void AddAssembly(IDictionary<string, Assembly> assemblies, Assembly assembly)
    {
        var key = assembly.FullName
            ?? assembly.GetName().Name
            ?? assembly.Location;

        assemblies.TryAdd(key, assembly);
    }

    private bool IsExcludedAssembly(string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return true;

        foreach (var pattern in _options.ExcludedAssemblyPatterns)
        {
            if (pattern.EndsWith('*'))
            {
                var prefix = pattern.TrimEnd('*');
                if (assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (string.Equals(assemblyName, pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerable<Type> ScanAssemblyForModules(Assembly assembly)
    {
        var types = assembly.GetTypes();
        return types.Where(IsValidModuleType);
    }

    private bool IsValidModuleType(Type type)
    {
        // Must be non-abstract, non-generic class implementing IAeroModule
        if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
            return false;

        if (!typeof(IAeroModule).IsAssignableFrom(type))
            return false;

        // Apply custom type filter if provided
        if (_options.TypeFilter != null && !_options.TypeFilter(type))
            return false;

        return true;
    }

    private static bool IsDisabledInProduction(Type type)
    {
        // Check if type has DisabledInProduction property via instance inspection
        // This is a heuristic - in production, modules should self-declare this
        try
        {
            // Try to find a public static property first (for perf)
            var prop = type.GetProperty("DisabledInProduction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                return (bool)(prop.GetValue(null) ?? false);
            }
        }
        catch
        {
            // Ignore reflection errors
        }

        return false;
    }

    private static ModuleDescriptor CreateDescriptor(Type type)
    {
        // Create a temporary instance to extract metadata
        // This follows the pattern used in ModuleExtensions
        IAeroModule? instance;
        try
        {
            instance = (IAeroModule?)Activator.CreateInstance(type);
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException($"Module type '{type.FullName}' must have a parameterless constructor.", ex);
        }

        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of module type '{type.FullName}'.");
        }

        var assembly = type.Assembly;
        var isUiModule = typeof(IUiModule).IsAssignableFrom(type);

        return new()
        {
            Name = instance.Name,
            Version = instance.Version,
            Author = instance.Author,
            ModuleType = type,
            Dependencies = instance.Dependencies,
            AssemblyName = assembly.GetName().Name ?? "Unknown",
            PhysicalPath = assembly.Location,
            IsUiModule = isUiModule,
            Order = instance.Order,
            Category = instance.Category,
            Tags = instance.Tags,
            DisabledInProduction = instance.DisabledInProduction
        };
    }
}
