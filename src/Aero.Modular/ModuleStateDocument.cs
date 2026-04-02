namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;

/// <summary>
/// Document representing the persisted state of a module.
/// </summary>
public sealed class ModuleStateDocument
{
    /// <summary>
    /// Creates a new module state document from a module descriptor.
    /// </summary>
    public static ModuleStateDocument FromDescriptor(ModuleDescriptor descriptor, bool isBuiltIn = false)
        => new()
        {
            Id = $"{ModuleIdPrefix}{descriptor.Name}",
            Name = descriptor.Name,
            Version = descriptor.Version,
            Author = descriptor.Author,
            Description = null,
            Order = (short)descriptor.Order,
            Category = descriptor.Category.ToList(),
            Tags = descriptor.Tags.ToList(),
            Disabled = false,
            DisabledInProduction = descriptor.DisabledInProduction,
            Dependencies = descriptor.Dependencies.ToList(),
            IsBuiltIn = isBuiltIn
        };

    public const string ModuleIdPrefix = "module:";

    /// <summary>
    /// Gets the Marten identity string for this document.
    /// Format: "module:{Name}"
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the module.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the module.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the module.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the module.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the load order of the module.
    /// Negative numbers are loaded before larger numbers.
    /// </summary>
    public short Order { get; set; }

    /// <summary>
    /// Gets or sets the categories the module belongs to.
    /// </summary>
    public List<string> Category { get; set; } = [];

    /// <summary>
    /// Gets or sets the tags associated with the module.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the module is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the module is disabled in production environments.
    /// </summary>
    public bool DisabledInProduction { get; set; }

    /// <summary>
    /// Gets or sets the module dependencies (by module name).
    /// </summary>
    public List<string> Dependencies { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this is a built-in module that cannot be uninstalled.
    /// </summary>
    public bool IsBuiltIn { get; set; }
}
