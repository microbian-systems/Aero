namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents the minimum bounding rectangle (MBR) for an R-Tree node.
/// </summary>
public class Mbr(Point min, Point max)
{
        /// <summary>
    /// Gets or sets the Min.
    /// </summary>
public Point Min { get; } = min;
        /// <summary>
    /// Gets or sets the Max.
    /// </summary>
public Point Max { get; } = max;

        /// <summary>
    /// Area method.
    /// </summary>
public double Area()
    {
        return (Max.X - Min.X) * (Max.Y - Min.Y);
    }

        /// <summary>
    /// Intersects method.
    /// </summary>
public bool Intersects(Mbr other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;
    }

        /// <summary>
    /// Contains method.
    /// </summary>
public bool Contains(Point point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y;
    }

        /// <summary>
    /// Enlargement method.
    /// </summary>
public double Enlargement(Point point)
    {
        double enlargedArea = (Math.Max(Max.X, point.X) - Math.Min(Min.X, point.X)) *
                              (Math.Max(Max.Y, point.Y) - Math.Min(Min.Y, point.Y));
        return enlargedArea - Area();
    }
}