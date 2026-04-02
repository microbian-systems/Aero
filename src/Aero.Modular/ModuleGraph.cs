namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;

/// <summary>
/// Represents the resolved dependency graph and load order of modules.
/// </summary>
public sealed class ModuleGraph
{
    public required IReadOnlyDictionary<string, ModuleDescriptor> Modules { get; init; }
    public required IReadOnlyList<ModuleDescriptor> LoadOrder { get; init; }

    public static ModuleGraph Empty() => new()
    {
        Modules = new Dictionary<string, ModuleDescriptor>(),
        LoadOrder = []
    };
}
