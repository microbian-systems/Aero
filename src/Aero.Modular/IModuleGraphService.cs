namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;

/// <summary>
/// Service responsible for building the module dependency graph and resolving load order.
/// </summary>
public interface IModuleGraphService
{
    /// <summary>
    /// Builds a module graph from discovered module descriptors.
    /// </summary>
    /// <param name="descriptors">The discovered module descriptors.</param>
    /// <returns>A module graph containing the resolved dependency order.</returns>
    /// <exception cref="ModuleDependencyException">Thrown when there are missing or circular dependencies.</exception>
    ModuleGraph BuildGraph(IReadOnlyList<ModuleDescriptor> descriptors);

    /// <summary>
    /// Validates the module descriptors without building the full graph.
    /// Checks for duplicate names and invalid dependency declarations.
    /// </summary>
    /// <param name="descriptors">The module descriptors to validate.</param>
    /// <returns>A validation result containing any errors found.</returns>
    ModuleValidationResult Validate(IReadOnlyList<ModuleDescriptor> descriptors);

    /// <summary>
    /// Gets the effective module set for a tenant, including all dependencies.
    /// </summary>
    /// <param name="graph">The full module graph.</param>
    /// <param name="enabledModuleNames">The names of modules explicitly enabled for the tenant.</param>
    /// <returns>A filtered graph containing only the effective modules for the tenant.</returns>
    ModuleGraph GetEffectiveModuleSet(ModuleGraph graph, IEnumerable<string> enabledModuleNames);
}

/// <summary>
/// Result of module validation containing any errors found.
/// </summary>
public sealed class ModuleValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ModuleValidationError> Errors { get; init; } = new();
}

/// <summary>
/// Represents a validation error for a module.
/// </summary>
public sealed class ModuleValidationError
{
    public required string ModuleName { get; init; }
    public required string ErrorType { get; init; }
    public required string Message { get; init; }
    public string? Details { get; init; }
}

/// <summary>
/// Exception thrown when there are issues with module dependencies.
/// </summary>
public class ModuleDependencyException : Exception
{
    public ModuleDependencyException(string message) : base(message) { }
    public ModuleDependencyException(string message, Exception inner) : base(message, inner) { }
    public string? OffendingModule { get; init; }
    public IReadOnlyList<string>? MissingDependencies { get; init; }
    public IReadOnlyList<string>? CycleMembers { get; init; }
}
