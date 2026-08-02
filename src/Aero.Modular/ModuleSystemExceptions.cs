namespace Aero.Modular;

/// <summary>
/// Exception thrown when a module system error occurs during discovery, validation, or loading.
/// </summary>
public abstract class ModuleSystemException : Exception
{
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleSystemException"/> class.
    /// </summary>
protected ModuleSystemException(string message) : base(message) { }
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleSystemException"/> class.
    /// </summary>
protected ModuleSystemException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception thrown when a duplicate module name is detected during discovery.
/// </summary>
public sealed class DuplicateModuleNameException(string moduleName, string firstAssembly, string secondAssembly)
    : ModuleSystemException(
        $"Duplicate module name '{moduleName}' detected. First defined in '{firstAssembly}', then in '{secondAssembly}'.")
{
        /// <summary>
    /// Gets or sets the Module Name.
    /// </summary>
public string ModuleName { get; } = moduleName;
        /// <summary>
    /// Gets or sets the First Assembly.
    /// </summary>
public string FirstAssembly { get; } = firstAssembly;
        /// <summary>
    /// Gets or sets the Second Assembly.
    /// </summary>
public string SecondAssembly { get; } = secondAssembly;
}

/// <summary>
/// Exception thrown when a module has a dependency that cannot be found.
/// </summary>
public sealed class MissingModuleDependencyException(
    string moduleName,
    string missingDependency,
    IEnumerable<string> availableModules)
    : ModuleSystemException(
        $"Module '{moduleName}' depends on '{missingDependency}' which was not found. Available modules: {string.Join(", ", availableModules)}.")
{
        /// <summary>
    /// Gets or sets the Module Name.
    /// </summary>
public string ModuleName { get; } = moduleName;
        /// <summary>
    /// Gets or sets the Missing Dependency.
    /// </summary>
public string MissingDependency { get; } = missingDependency;
        /// <summary>
    /// Gets or sets the Available Modules.
    /// </summary>
public IReadOnlyList<string> AvailableModules { get; } = availableModules.ToList().AsReadOnly();
}

/// <summary>
/// Exception thrown when a circular dependency is detected in the module graph.
/// </summary>
public sealed class CircularDependencyException(IEnumerable<string> cyclePath) : ModuleSystemException(
    $"Circular dependency detected: {string.Join(" -> ", cyclePath)} -> {cyclePath.FirstOrDefault()}.")
{
        /// <summary>
    /// Gets or sets the Cycle Path.
    /// </summary>
public IReadOnlyList<string> CyclePath { get; } = cyclePath.ToList().AsReadOnly();
}

/// <summary>
/// Exception thrown when an assembly fails to load during module discovery.
/// </summary>
public sealed class ModuleAssemblyLoadException(string assemblyName, string? assemblyPath, Exception inner)
    : ModuleSystemException(
        $"Failed to load assembly '{assemblyName}'{(assemblyPath != null ? $" from '{assemblyPath}'" : "")}.", inner)
{
        /// <summary>
    /// Gets or sets the Assembly Name.
    /// </summary>
public string AssemblyName { get; } = assemblyName;
        /// <summary>
    /// Gets or sets the Assembly Path.
    /// </summary>
public string? AssemblyPath { get; } = assemblyPath;
}

/// <summary>
/// Exception thrown when a tenant enables a module that was not discovered.
/// </summary>
public sealed class UnknownModuleException(string moduleName, string? tenantId = null) : ModuleSystemException(
    tenantId != null
        ? $"Tenant '{tenantId}' enabled unknown module '{moduleName}'."
        : $"Unknown module '{moduleName}'.")
{
        /// <summary>
    /// Gets or sets the Module Name.
    /// </summary>
public string ModuleName { get; } = moduleName;
        /// <summary>
    /// Gets or sets the Tenant Id.
    /// </summary>
public string? TenantId { get; } = tenantId;
}
