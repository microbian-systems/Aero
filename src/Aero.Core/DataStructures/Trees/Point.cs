namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a point in 2D space.
/// </summary>
public class Point(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;

    public double DistanceTo(Point other)
    {
        return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    }
}
