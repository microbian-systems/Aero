namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of the module graph service with topological sorting.
/// </summary>
public sealed class ModuleGraphService(ILogger<ModuleGraphService> logger) : IModuleGraphService
{
    /// <inheritdoc/>
    public ModuleGraph BuildGraph(IReadOnlyList<ModuleDescriptor> descriptors)
    {
        if (descriptors.Count == 0)
        {
            return ModuleGraph.Empty();
        }

        var modulesByName = descriptors.ToDictionary(
            d => d.Name,
            d => d,
            StringComparer.OrdinalIgnoreCase);

        // Validate dependencies exist
        var validation = Validate(descriptors);
        if (!validation.IsValid)
        {
            var error = validation.Errors.First();
            throw new ModuleDependencyException(error.Message)
            {
                OffendingModule = error.ModuleName
            };
        }

        // Perform topological sort using Kahn's algorithm
        var loadOrder = TopologicalSort(descriptors, modulesByName);

        logger.LogInformation("Built module graph with {Count} modules in load order.", loadOrder.Count);

        return new ModuleGraph
        {
            Modules = modulesByName,
            LoadOrder = loadOrder
        };
    }

    /// <inheritdoc/>
    public ModuleValidationResult Validate(IReadOnlyList<ModuleDescriptor> descriptors)
    {
        var result = new ModuleValidationResult();
        var moduleNames = new HashSet<string>(descriptors.Select(d => d.Name), StringComparer.OrdinalIgnoreCase);

        // Check for duplicate names
        var nameGroups = descriptors.GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in nameGroups)
        {
            var assemblies = string.Join(", ", group.Select(g => g.AssemblyName));
            result.Errors.Add(new ModuleValidationError
            {
                ModuleName = group.Key,
                ErrorType = "DuplicateName",
                Message = $"Duplicate module name '{group.Key}' found in assemblies: {assemblies}.",
                Details = "Each module must have a unique name."
            });
        }

        // Check for missing dependencies
        foreach (var descriptor in descriptors)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                if (!moduleNames.Contains(dependency))
                {
                    result.Errors.Add(new ModuleValidationError
                    {
                        ModuleName = descriptor.Name,
                        ErrorType = "MissingDependency",
                        Message = $"Module '{descriptor.Name}' depends on '{dependency}' which was not found.",
                        Details = $"Available modules: {string.Join(", ", moduleNames)}."
                    });
                }
            }
        }

        // Check for circular dependencies
        var cycle = FindCycle(descriptors);
        if (cycle != null)
        {
            result.Errors.Add(new ModuleValidationError
            {
                ModuleName = cycle.First(),
                ErrorType = "CircularDependency",
                Message = $"Circular dependency detected: {string.Join(" -> ", cycle)} -> {cycle.First()}.",
                Details = "Module dependencies must form a directed acyclic graph."
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public ModuleGraph GetEffectiveModuleSet(ModuleGraph graph, IEnumerable<string> enabledModuleNames)
    {
        var enabledNames = new HashSet<string>(enabledModuleNames, StringComparer.OrdinalIgnoreCase);
        var effectiveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Recursively add all dependencies
        void AddWithDependencies(string moduleName)
        {
            if (effectiveNames.Contains(moduleName))
                return;

            if (!graph.Modules.TryGetValue(moduleName, out var descriptor))
            {
                throw new UnknownModuleException(moduleName);
            }

            effectiveNames.Add(moduleName);

            foreach (var dependency in descriptor.Dependencies)
            {
                AddWithDependencies(dependency);
            }
        }

        foreach (var name in enabledNames)
        {
            AddWithDependencies(name);
        }

        // Filter the modules dictionary
        var effectiveModules = graph.Modules
            .Where(kvp => effectiveNames.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        // Recompute load order for effective set
        var effectiveList = effectiveModules.Values.ToList();
        var loadOrder = TopologicalSort(effectiveList, effectiveModules);

        logger.LogInformation("Effective module set contains {Count} modules (from {Enabled} enabled).",
            effectiveNames.Count, enabledNames.Count);

        return new ModuleGraph
        {
            Modules = effectiveModules,
            LoadOrder = loadOrder
        };
    }

    private IReadOnlyList<ModuleDescriptor> TopologicalSort(
        IReadOnlyList<ModuleDescriptor> descriptors,
        IReadOnlyDictionary<string, ModuleDescriptor> modulesByName)
    {
        // Build adjacency list and compute indegrees
        var adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var indegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in descriptors)
        {
            adjacency[descriptor.Name] = new List<string>();
            indegree[descriptor.Name] = 0;
        }

        foreach (var descriptor in descriptors)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                if (modulesByName.TryGetValue(dependency, out var _))
                {
                    // dependency -> descriptor (dependency must come first)
                    adjacency[dependency].Add(descriptor.Name);
                    indegree[descriptor.Name]++;
                }
            }
        }

        // Kahn's algorithm with stable ordering
        var queue = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ModuleDescriptor>();

        foreach (var kvp in indegree)
        {
            if (kvp.Value == 0)
            {
                queue.Add(kvp.Key);
            }
        }

        while (queue.Count > 0)
        {
            // Take the first (alphabetically) for deterministic ordering
            var name = queue.Min!;
            queue.Remove(name);

            result.Add(modulesByName[name]);

            foreach (var dependent in adjacency[name])
            {
                indegree[dependent]--;
                if (indegree[dependent] == 0)
                {
                    queue.Add(dependent);
                }
            }
        }

        // If we couldn't process all modules, there's a cycle
        if (result.Count != descriptors.Count)
        {
            var cycle = FindCycle(descriptors);
            var cycleStr = cycle != null ? string.Join(" -> ", cycle) : "unknown";
            throw new ModuleDependencyException($"Circular dependency detected in module graph: {cycleStr}.")
            {
                CycleMembers = cycle?.ToList()
            };
        }

        return result.AsReadOnly();
    }

    private List<string>? FindCycle(IReadOnlyList<ModuleDescriptor> descriptors)
    {
        var modulesByName = descriptors.ToDictionary(
            d => d.Name,
            d => d,
            StringComparer.OrdinalIgnoreCase);

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recursionStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();

        foreach (var descriptor in descriptors)
        {
            if (!visited.Contains(descriptor.Name))
            {
                var cycle = FindCycleDFS(descriptor.Name, visited, recursionStack, path, modulesByName);
                if (cycle != null)
                    return cycle;
            }
        }

        return null;
    }

    private List<string>? FindCycleDFS(
        string moduleName,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string> path,
        IReadOnlyDictionary<string, ModuleDescriptor> modulesByName)
    {
        visited.Add(moduleName);
        recursionStack.Add(moduleName);
        path.Add(moduleName);

        if (modulesByName.TryGetValue(moduleName, out var descriptor))
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                if (!visited.Contains(dependency))
                {
                    var cycle = FindCycleDFS(dependency, visited, recursionStack, path, modulesByName);
                    if (cycle != null)
                        return cycle;
                }
                else if (recursionStack.Contains(dependency))
                {
                    // Found a cycle - extract the cycle from the path
                    var cycleStart = path.IndexOf(dependency);
                    return path.Skip(cycleStart).ToList();
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        recursionStack.Remove(moduleName);
        return null;
    }
}
