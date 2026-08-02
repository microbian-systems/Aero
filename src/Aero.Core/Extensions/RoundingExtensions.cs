namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for RoundingExtensions.
/// </summary>
public static class RoundingExtensions
{
        /// <summary>
    /// RoundToTwoDecimalPlaces method.
    /// </summary>
public static double RoundToTwoDecimalPlaces(this double number)
    {
        var rounded = Math.Round(number, 2);

        return rounded;
    }

        /// <summary>
    /// RoundUpOrDown method.
    /// </summary>
public static double RoundUpOrDown(this double number)
    {
        var roundedDoubleNumber = RoundedDivision(number);

        return roundedDoubleNumber;
    }

    private static double RoundedDivision(double number)
    {
        double divisor = 1;
        var div = number / divisor;
        var floor = Math.Floor(div);
        var celing = Math.Ceiling(div);
        var difference = (div - floor);

        return difference < 0.5 ? floor : celing;
    }

        /// <summary>
    /// RoundToOneDecimalPlaces method.
    /// </summary>
public static double RoundToOneDecimalPlaces(this double number)
    {
        var rounded = Math.Round(number, 2);

        return rounded;
    }

        /// <summary>
    /// RoundToTwoDecimalPlaces method.
    /// </summary>
public static decimal RoundToTwoDecimalPlaces(this decimal number)
    {
        var rounded = Math.Round(number, 2);

        return rounded;
    }
}