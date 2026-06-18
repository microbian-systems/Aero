using Aero.Social.Plugs;

namespace Aero.Social.Abstractions;

/// <summary>
/// Provides pre-resolved plug metadata for social providers.
/// The production implementation uses provider-declared plugs via
/// <see cref="SocialProviderBase.GetDeclaredPlugs"/> instead of
/// runtime reflection or assembly scanning.
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
/// Information about a declared plug.
/// </summary>
/// <remarks>
/// Plugs are declared by providers via <see cref="SocialProviderBase.GetDeclaredPlugs"/>
/// rather than discovered via runtime attribute reflection. The <see cref="Execute"/>
/// delegate is called directly instead of using <c>MethodInfo.Invoke()</c>.
/// </remarks>
public class PlugInfo
{
    /// <summary>
    /// Gets or sets the delegate to invoke when this plug executes.
    /// Called with the execution context and cancellation token.
    /// Returns the execution result.
    /// </summary>
    public Func<PlugExecutionContext, CancellationToken, Task<PlugExecutionResult>>? Execute { get; set; }

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
