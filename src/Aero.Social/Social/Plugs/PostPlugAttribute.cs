namespace Aero.Social.Plugs;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class PostPlugAttribute(
    string identifier,
    string title,
    string description,
    int runEveryMilliseconds,
    int totalRuns = 0)
    : Attribute
{
    public string Identifier { get; } = identifier ?? throw new ArgumentNullException(nameof(identifier));
    public string Title { get; } = title ?? throw new ArgumentNullException(nameof(title));
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));
    public int RunEveryMilliseconds { get; } = runEveryMilliseconds;
    public int TotalRuns { get; } = totalRuns;
    public List<PlugField> Fields { get; } = new();

    /// <summary>
    /// Specifies the trigger condition for the plug (e.g., "likes", "comments", "shares")
    /// </summary>
    public string? TriggerOn { get; set; }
    
    /// <summary>
    /// The threshold value for the trigger (e.g., number of likes needed)
    /// </summary>
    public int TriggerThreshold { get; set; }

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
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
    public string? Placeholder { get; } = placeholder;
    public string? Description { get; } = description;
}
