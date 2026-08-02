namespace Aero.Social.Plugs;

/// <summary>
/// Represents a class for PlugAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class PlugAttribute(
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
    /// AddField method.
    /// </summary>
public PlugAttribute AddField(PlugField field)
    {
        Fields.Add(field);
        return this;
    }
}

/// <summary>
/// Represents a class for PlugField.
/// </summary>
public class PlugField(
    string name,
    string type,
    string? placeholder = null,
    string? description = null)
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
        /// <summary>
    /// Gets or sets the Validations.
    /// </summary>
public List<IFieldValidation> Validations { get; } = new();

        /// <summary>
    /// AddValidation method.
    /// </summary>
public PlugField AddValidation(IFieldValidation validation)
    {
        Validations.Add(validation);
        return this;
    }
}

/// <summary>
/// Defines an interface for IFieldValidation.
/// </summary>
public interface IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
string Type { get; }
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
string? ErrorMessage { get; }
        /// <summary>
    /// Validate method.
    /// </summary>
bool Validate(object? value);
}

/// <summary>
/// Represents a class for RequiredValidation.
/// </summary>
public class RequiredValidation(string? errorMessage = null) : IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type => "required";
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
public string? ErrorMessage { get; } = errorMessage ?? "This field is required";

        /// <summary>
    /// Validate method.
    /// </summary>
public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is string str) return !string.IsNullOrWhiteSpace(str);
        return true;
    }
}

/// <summary>
/// Represents a class for MinValueValidation.
/// </summary>
public class MinValueValidation(int minValue, string? errorMessage = null) : IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type => "min";
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
public string? ErrorMessage { get; } = errorMessage ?? $"Value must be at least {minValue}";
        /// <summary>
    /// Gets or sets the Min Value.
    /// </summary>
public int MinValue { get; } = minValue;

        /// <summary>
    /// Validate method.
    /// </summary>
public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is int intValue) return intValue >= MinValue;
        if (value is long longValue) return longValue >= MinValue;
        if (value is double doubleValue) return doubleValue >= MinValue;
        if (value is decimal decimalValue) return decimalValue >= MinValue;
        return false;
    }
}

/// <summary>
/// Represents a class for MaxValueValidation.
/// </summary>
public class MaxValueValidation(int maxValue, string? errorMessage = null) : IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type => "max";
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
public string? ErrorMessage { get; } = errorMessage ?? $"Value must be at most {maxValue}";
        /// <summary>
    /// Gets or sets the Max Value.
    /// </summary>
public int MaxValue { get; } = maxValue;

        /// <summary>
    /// Validate method.
    /// </summary>
public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is int intValue) return intValue <= MaxValue;
        if (value is long longValue) return longValue <= MaxValue;
        if (value is double doubleValue) return doubleValue <= MaxValue;
        if (value is decimal decimalValue) return decimalValue <= MaxValue;
        return false;
    }
}

/// <summary>
/// Represents a class for RangeValidation.
/// </summary>
public class RangeValidation(int minValue, int maxValue, string? errorMessage = null) : IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type => "range";
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
public string? ErrorMessage { get; } = errorMessage ?? $"Value must be between {minValue} and {maxValue}";
        /// <summary>
    /// Gets or sets the Min Value.
    /// </summary>
public int MinValue { get; } = minValue;
        /// <summary>
    /// Gets or sets the Max Value.
    /// </summary>
public int MaxValue { get; } = maxValue;

        /// <summary>
    /// Validate method.
    /// </summary>
public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is int intValue) return intValue >= MinValue && intValue <= MaxValue;
        if (value is long longValue) return longValue >= MinValue && longValue <= MaxValue;
        if (value is double doubleValue) return doubleValue >= MinValue && doubleValue <= MaxValue;
        if (value is decimal decimalValue) return decimalValue >= MinValue && decimalValue <= MaxValue;
        return false;
    }
}

/// <summary>
/// Represents a class for PatternValidation.
/// </summary>
public class PatternValidation(string pattern, string? errorMessage = null) : IFieldValidation
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type => "pattern";
        /// <summary>
    /// Gets or sets the Error Message.
    /// </summary>
public string? ErrorMessage { get; } = errorMessage ?? "Value does not match the required pattern";
        /// <summary>
    /// Gets or sets the Pattern.
    /// </summary>
public string Pattern { get; } = pattern ?? throw new ArgumentNullException(nameof(pattern));

        /// <summary>
    /// Validate method.
    /// </summary>
public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is not string str) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(str, Pattern);
    }
}
