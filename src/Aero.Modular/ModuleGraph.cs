namespace Aero.Modular;

/// <summary>
/// Represents the resolved dependency graph and load order of modules.
/// </summary>
public sealed class ModuleGraph
{
        /// <summary>
    /// Gets or sets the Modules.
    /// </summary>
public required IReadOnlyDictionary<string, ModuleDescriptor> Modules { get; init; }
        /// <summary>
    /// Gets or sets the Load Order.
    /// </summary>
public required IReadOnlyList<ModuleDescriptor> LoadOrder { get; init; }

        /// <summary>
    /// Empty method.
    /// </summary>
public static ModuleGraph Empty() => new()
    {
        Modules = new Dictionary<string, ModuleDescriptor>(),
        LoadOrder = []
    };
}
