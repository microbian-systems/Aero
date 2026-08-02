namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for Numbers.
/// </summary>
public static class Numbers
{
        /// <summary>
    /// RaiseTo method.
    /// </summary>
public static decimal RaiseTo(decimal start, decimal nearest)
    {
        return Math.Ceiling(start / nearest) * nearest;
    }
}