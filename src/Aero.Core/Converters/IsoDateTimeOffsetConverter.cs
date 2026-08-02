using System.Globalization;

namespace Aero.Core.Converters;

/// <summary>
/// Represents a class for IsoDateTimeOffsetConverter.
/// </summary>
public class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
        /// <summary>
    /// CanConvert method.
    /// </summary>
public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

    private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
    private string? _dateTimeFormat;
    private CultureInfo? _culture;

        /// <summary>
    /// Gets or sets the Date Time Styles.
    /// </summary>
public DateTimeStyles DateTimeStyles
    {
        get => _dateTimeStyles;
        set => _dateTimeStyles = value;
    }

        /// <summary>
    /// Gets or sets the Date Time Format.
    /// </summary>
public string? DateTimeFormat
    {
        get => _dateTimeFormat ?? string.Empty;
        set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
    }

        /// <summary>
    /// Gets or sets the Culture.
    /// </summary>
public CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

        /// <summary>
    /// Write method.
    /// </summary>
public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        string text;


        if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
            || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
        {
            value = value.ToUniversalTime();
        }

        text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

        writer.WriteStringValue(text);
    }

        /// <summary>
    /// Read method.
    /// </summary>
public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dateText = reader.GetString();

        if (string.IsNullOrEmpty(dateText) == false)
        {
            if (!string.IsNullOrEmpty(_dateTimeFormat))
            {
                return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
            }
            else
            {
                return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
            }
        }
        else
        {
            return default(DateTimeOffset);
        }
    }


        /// <summary>
    /// Singleton.
    /// </summary>
public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
}