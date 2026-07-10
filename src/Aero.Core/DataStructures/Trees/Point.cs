namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a point in 2D space.
/// </summary>
public class Point(double x, double y)
{
        /// <summary>
    /// Gets or sets the X.
    /// </summary>
public double X { get; } = x;
        /// <summary>
    /// Gets or sets the Y.
    /// </summary>
public double Y { get; } = y;

        /// <summary>
    /// DistanceTo method.
    /// </summary>
public double DistanceTo(Point other)
    {
        return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    }
}
