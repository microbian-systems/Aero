namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for StringToIntConverter.
/// </summary>
public class StringToIntConverter : JsonConverter<int>
{
        /// <summary>
    /// CanConvert method.
    /// </summary>
public override bool CanConvert(Type t) => t == typeof(int);

        /// <summary>
    /// Read method.
    /// </summary>
public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        int l;
        if (int.TryParse(value, out l))
        {
            return l;
        }

        throw new Exception("Cannot unmarshal type int");
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
        return;
    }

        /// <summary>
    /// Singleton.
    /// </summary>
public static readonly StringToIntConverter Singleton = new StringToIntConverter();
}