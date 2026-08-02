namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for TimeOnlyConverter.
/// </summary>
public class TimeOnlyConverter(string? serializationFormat) : JsonConverter<TimeOnly>
{
    private readonly string serializationFormat = serializationFormat ?? "HH:mm:ss.fff";

        /// <summary>
    /// Initializes a new instance of the <see cref="TimeOnlyConverter"/> class.
    /// </summary>
public TimeOnlyConverter() : this(null)
    {
    }

        /// <summary>
    /// Read method.
    /// </summary>
public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.Parse(value!);
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(serializationFormat));
}