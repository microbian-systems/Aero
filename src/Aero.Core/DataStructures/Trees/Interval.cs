namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents an interval with a start and an end.
/// </summary>
public class Interval(int start, int end) : IComparable<Interval>
{
        /// <summary>
    /// Gets or sets the Start.
    /// </summary>
public int Start { get; } = start;
        /// <summary>
    /// Gets or sets the End.
    /// </summary>
public int End { get; } = end;

        /// <summary>
    /// CompareTo method.
    /// </summary>
public int CompareTo(Interval other)
    {
        return Start.CompareTo(other.Start);
    }

        /// <summary>
    /// Overlaps method.
    /// </summary>
public bool Overlaps(Interval other)
    {
        return Start < other.End && End > other.Start;
    }
}