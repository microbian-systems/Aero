namespace Aero.Modular;

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
        /// <summary>
    /// Gets or sets the Is Valid.
    /// </summary>
public bool IsValid => Errors.Count == 0;
        /// <summary>
    /// Gets or sets the Errors.
    /// </summary>
public List<ModuleValidationError> Errors { get; init; } = new();
}

/// <summary>
/// Represents a validation error for a module.
/// </summary>
public sealed class ModuleValidationError
{
        /// <summary>
    /// Gets or sets the Module Name.
    /// </summary>
public required string ModuleName { get; init; }
        /// <summary>
    /// Gets or sets the Error Type.
    /// </summary>
public required string ErrorType { get; init; }
        /// <summary>
    /// Gets or sets the Message.
    /// </summary>
public required string Message { get; init; }
        /// <summary>
    /// Gets or sets the Details.
    /// </summary>
public string? Details { get; init; }
}

/// <summary>
/// Exception thrown when there are issues with module dependencies.
/// </summary>
public class ModuleDependencyException : Exception
{
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleDependencyException"/> class.
    /// </summary>
public ModuleDependencyException(string message) : base(message) { }
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleDependencyException"/> class.
    /// </summary>
public ModuleDependencyException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
    /// Gets or sets the Offending Module.
    /// </summary>
public string? OffendingModule { get; init; }
        /// <summary>
    /// Gets or sets the Missing Dependencies.
    /// </summary>
public IReadOnlyList<string>? MissingDependencies { get; init; }
        /// <summary>
    /// Gets or sets the Cycle Members.
    /// </summary>
public IReadOnlyList<string>? CycleMembers { get; init; }
}
