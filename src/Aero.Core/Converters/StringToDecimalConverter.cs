namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for StringToDecimalConverter.
/// </summary>
public class StringToDecimalConverter : JsonConverter<decimal>
{
        /// <summary>
    /// CanConvert method.
    /// </summary>
public override bool CanConvert(Type t) => t == typeof(decimal);

        /// <summary>
    /// Read method.
    /// </summary>
public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        decimal l;
        if (decimal.TryParse(value, out l))
        {
            return l;
        }

        throw new Exception("Cannot unmarshal type decimal");
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
        return;
    }

        /// <summary>
    /// Singleton.
    /// </summary>
public static readonly StringToDecimalConverter Singleton = new StringToDecimalConverter();
}