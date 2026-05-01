namespace Aero.Social.Plugs;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class PlugAttribute(
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

    public PlugAttribute AddField(PlugField field)
    {
        Fields.Add(field);
        return this;
    }
}

public class PlugField(
    string name,
    string type,
    string? placeholder = null,
    string? description = null)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
    public string? Placeholder { get; } = placeholder;
    public string? Description { get; } = description;
    public List<IFieldValidation> Validations { get; } = new();

    public PlugField AddValidation(IFieldValidation validation)
    {
        Validations.Add(validation);
        return this;
    }
}

public interface IFieldValidation
{
    string Type { get; }
    string? ErrorMessage { get; }
    bool Validate(object? value);
}

public class RequiredValidation(string? errorMessage = null) : IFieldValidation
{
    public string Type => "required";
    public string? ErrorMessage { get; } = errorMessage ?? "This field is required";

    public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is string str) return !string.IsNullOrWhiteSpace(str);
        return true;
    }
}

public class MinValueValidation(int minValue, string? errorMessage = null) : IFieldValidation
{
    public string Type => "min";
    public string? ErrorMessage { get; } = errorMessage ?? $"Value must be at least {minValue}";
    public int MinValue { get; } = minValue;

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

public class MaxValueValidation(int maxValue, string? errorMessage = null) : IFieldValidation
{
    public string Type => "max";
    public string? ErrorMessage { get; } = errorMessage ?? $"Value must be at most {maxValue}";
    public int MaxValue { get; } = maxValue;

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

public class RangeValidation(int minValue, int maxValue, string? errorMessage = null) : IFieldValidation
{
    public string Type => "range";
    public string? ErrorMessage { get; } = errorMessage ?? $"Value must be between {minValue} and {maxValue}";
    public int MinValue { get; } = minValue;
    public int MaxValue { get; } = maxValue;

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

public class PatternValidation(string pattern, string? errorMessage = null) : IFieldValidation
{
    public string Type => "pattern";
    public string? ErrorMessage { get; } = errorMessage ?? "Value does not match the required pattern";
    public string Pattern { get; } = pattern ?? throw new ArgumentNullException(nameof(pattern));

    public bool Validate(object? value)
    {
        if (value == null) return false;
        if (value is not string str) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(str, Pattern);
    }
}
