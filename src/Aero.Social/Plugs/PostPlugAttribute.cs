namespace Aero.Social.Plugs;

/// <summary>
/// Represents a class for PostPlugAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class PostPlugAttribute(
    string identifier,
    string title,
    string description,
    int runEveryMilliseconds,
    int totalRuns = 0)
    : Attribute
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public string Identifier { get; } = identifier ?? throw new ArgumentNullException(nameof(identifier));
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
public string Title { get; } = title ?? throw new ArgumentNullException(nameof(title));
        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));
        /// <summary>
    /// Gets or sets the Run Every Milliseconds.
    /// </summary>
public int RunEveryMilliseconds { get; } = runEveryMilliseconds;
        /// <summary>
    /// Gets or sets the Total Runs.
    /// </summary>
public int TotalRuns { get; } = totalRuns;
        /// <summary>
    /// Gets or sets the Fields.
    /// </summary>
public List<PlugField> Fields { get; } = new();

    /// <summary>
    /// Specifies the trigger condition for the plug (e.g., "likes", "comments", "shares")
    /// </summary>
    public string? TriggerOn { get; set; }
    
    /// <summary>
    /// The threshold value for the trigger (e.g., number of likes needed)
    /// </summary>
    public int TriggerThreshold { get; set; }

        /// <summary>
    /// AddField method.
    /// </summary>
public PostPlugAttribute AddField(PlugField field)
    {
        Fields.Add(field);
        return this;
    }
}

/// <summary>
/// Attribute to define a field for a plug. Can be applied multiple times to a method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class PlugFieldAttribute(
    string name,
    string type,
    string? placeholder = null,
    string? description = null)
    : Attribute
{
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
        /// <summary>
    /// Gets or sets the Placeholder.
    /// </summary>
public string? Placeholder { get; } = placeholder;
        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
public string? Description { get; } = description;
}
