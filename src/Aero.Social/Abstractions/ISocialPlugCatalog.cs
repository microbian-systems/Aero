using System.Reflection;
using Aero.Social.Plugs;

namespace Aero.Social.Abstractions;

/// <summary>
/// Provides pre-resolved plug metadata for social providers,
/// eliminating runtime method scanning for discovery.
/// The production implementation is source-generated from
/// <c>[Plug]</c> and <c>[PostPlug]</c> attributes.
/// </summary>
public interface ISocialPlugCatalog
{
    /// <summary>
    /// Returns all plugs for the given provider type.
    /// </summary>
    IReadOnlyList<PlugInfo> GetPlugs(Type providerType);

    /// <summary>
    /// Finds a plug by identifier for the given provider type.
    /// </summary>
    PlugInfo? GetPlug(Type providerType, string identifier);
}

/// <summary>
/// Information about a discovered plug.
/// </summary>
public class PlugInfo
{
    /// <summary>
    /// Gets or sets the method info for the plug.
    /// </summary>
    public MethodInfo Method { get; set; } = null!;

    /// <summary>
    /// Gets or sets the plug attribute (for regular plugs).
    /// </summary>
    public PlugAttribute? Attribute { get; set; }

    /// <summary>
    /// Gets or sets the post plug attribute (for post-processing plugs).
    /// </summary>
    public PostPlugAttribute? PostPlugAttribute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a post-processing plug.
    /// </summary>
    public bool IsPostPlug { get; set; }

    /// <summary>
    /// Gets the identifier for this plug.
    /// </summary>
    public string Identifier => IsPostPlug
        ? PostPlugAttribute?.Identifier ?? string.Empty
        : Attribute?.Identifier ?? string.Empty;

    /// <summary>
    /// Gets the title for this plug.
    /// </summary>
    public string Title => IsPostPlug
        ? PostPlugAttribute?.Title ?? string.Empty
        : Attribute?.Title ?? string.Empty;

    /// <summary>
    /// Gets the description for this plug.
    /// </summary>
    public string Description => IsPostPlug
        ? PostPlugAttribute?.Description ?? string.Empty
        : Attribute?.Description ?? string.Empty;
}
