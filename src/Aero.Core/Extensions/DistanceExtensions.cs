namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for DistanceExtensions.
/// </summary>
public static class DistanceExtensions
{
        /// <summary>
    /// FormatKilometres method.
    /// </summary>
public static double FormatKilometres(this double kilometres)
    {
        var roundedToTwoDecimalPlaces = kilometres.RoundToTwoDecimalPlaces();

        if (roundedToTwoDecimalPlaces <= 1)
            return roundedToTwoDecimalPlaces;
            
        var roundedUpOrDown = roundedToTwoDecimalPlaces.RoundUpOrDown();

        return roundedUpOrDown;
    }

        /// <summary>
    /// KilometresToMetres method.
    /// </summary>
public static double KilometresToMetres(this double kilometres)
    {
        var result = (1000D * kilometres);
        var roundedToTwoDecimalPlaces = result.RoundToTwoDecimalPlaces();

        if (roundedToTwoDecimalPlaces <= 1)
            return roundedToTwoDecimalPlaces;

        var roundedUpOrDown = roundedToTwoDecimalPlaces.RoundUpOrDown();

        return roundedUpOrDown;
    }

        /// <summary>
    /// KilometresToMiles method.
    /// </summary>
public static double KilometresToMiles(this double kilometres)
    {
        var result = (kilometres / 1.609344);
        var roundedToTwoDecimalPlaces = result.RoundToTwoDecimalPlaces();

        if (roundedToTwoDecimalPlaces <= 1)
            return roundedToTwoDecimalPlaces;

        var roundedUpOrDown = roundedToTwoDecimalPlaces.RoundUpOrDown();

        return roundedUpOrDown;
    }

        /// <summary>
    /// MetersToMiles method.
    /// </summary>
public static double MetersToMiles(this double meters)
    {
        var result = (meters * 0.000621371);
        var roundedToTwoDecimalPlaces = result.RoundToTwoDecimalPlaces();

        if (roundedToTwoDecimalPlaces <= 1)
            return roundedToTwoDecimalPlaces;

        var roundedUpOrDown = roundedToTwoDecimalPlaces.RoundUpOrDown();

        return roundedUpOrDown;
    }
}