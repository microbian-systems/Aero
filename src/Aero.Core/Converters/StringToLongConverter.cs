namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for StringToLongConverter.
/// </summary>
public class StringToLongConverter : JsonConverter<long>
{
        /// <summary>
    /// CanConvert method.
    /// </summary>
public override bool CanConvert(Type t) => t == typeof(long);

        /// <summary>
    /// Read method.
    /// </summary>
public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        long l;
        if (Int64.TryParse(value, out l))
        {
            return l;
        }

        throw new Exception("Cannot unmarshal type long");
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
        return;
    }

        /// <summary>
    /// Singleton.
    /// </summary>
public static readonly StringToLongConverter Singleton = new StringToLongConverter();
}