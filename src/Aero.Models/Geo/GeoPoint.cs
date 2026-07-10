namespace Aero.Models.Geo;

/// <summary>
/// Represents a record for GeoPoint.
/// </summary>
public record GeoPoint(double Latitude, double Longitude)
{
        /// <summary>
    /// Gets or sets the Latitude.
    /// </summary>
public double Latitude { get; } = Latitude;
        /// <summary>
    /// Gets or sets the Longitude.
    /// </summary>
public double Longitude { get; } = Longitude;
}