namespace Aero.Cms.Web.Core.Modules;

/// <summary>
/// Exception thrown when a module system error occurs during discovery, validation, or loading.
/// </summary>
public abstract class ModuleSystemException : Exception
{
    protected ModuleSystemException(string message) : base(message) { }
    protected ModuleSystemException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception thrown when a duplicate module name is detected during discovery.
/// </summary>
public sealed class DuplicateModuleNameException(string moduleName, string firstAssembly, string secondAssembly)
    : ModuleSystemException(
        $"Duplicate module name '{moduleName}' detected. First defined in '{firstAssembly}', then in '{secondAssembly}'.")
{
    public string ModuleName { get; } = moduleName;
    public string FirstAssembly { get; } = firstAssembly;
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
    public string ModuleName { get; } = moduleName;
    public string MissingDependency { get; } = missingDependency;
    public IReadOnlyList<string> AvailableModules { get; } = availableModules.ToList().AsReadOnly();
}

/// <summary>
/// Exception thrown when a circular dependency is detected in the module graph.
/// </summary>
public sealed class CircularDependencyException(IEnumerable<string> cyclePath) : ModuleSystemException(
    $"Circular dependency detected: {string.Join(" -> ", cyclePath)} -> {cyclePath.FirstOrDefault()}.")
{
    public IReadOnlyList<string> CyclePath { get; } = cyclePath.ToList().AsReadOnly();
}

/// <summary>
/// Exception thrown when an assembly fails to load during module discovery.
/// </summary>
public sealed class ModuleAssemblyLoadException(string assemblyName, string? assemblyPath, Exception inner)
    : ModuleSystemException(
        $"Failed to load assembly '{assemblyName}'{(assemblyPath != null ? $" from '{assemblyPath}'" : "")}.", inner)
{
    public string AssemblyName { get; } = assemblyName;
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
    public string ModuleName { get; } = moduleName;
    public string? TenantId { get; } = tenantId;
}
