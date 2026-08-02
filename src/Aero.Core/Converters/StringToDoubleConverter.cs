namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for StringToDoubleConverter.
/// </summary>
public class StringToDoubleConverter : JsonConverter<double>
{
        /// <summary>
    /// CanConvert method.
    /// </summary>
public override bool CanConvert(Type t) => t == typeof(double);

        /// <summary>
    /// Read method.
    /// </summary>
public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        double l;
        if (double.TryParse(value, out l))
        {
            return l;
        }
        return 0;
        //throw new Exception("Cannot unmarshal type double");
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
        return;
    }

        /// <summary>
    /// Singleton.
    /// </summary>
public static readonly StringToDoubleConverter Singleton = new StringToDoubleConverter();
}