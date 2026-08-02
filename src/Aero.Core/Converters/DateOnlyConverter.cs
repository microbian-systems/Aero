namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for DateOnlyConverter.
/// </summary>
public class DateOnlyConverter(string? serializationFormat) : JsonConverter<DateOnly>
{
    private readonly string serializationFormat = serializationFormat ?? "yyyy-MM-dd";

        /// <summary>
    /// Initializes a new instance of the <see cref="DateOnlyConverter"/> class.
    /// </summary>
public DateOnlyConverter() : this(null)
    {
    }

        /// <summary>
    /// Read method.
    /// </summary>
public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.Parse(value!);
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(serializationFormat));
}