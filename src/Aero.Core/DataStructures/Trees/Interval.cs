namespace Aero.DataStructures.Trees;

/// <summary>
/// Represents an interval with a start and an end.
/// </summary>
public class Interval(int start, int end) : IComparable<Interval>
{
    public int Start { get; } = start;
    public int End { get; } = end;

    public int CompareTo(Interval other)
    {
        return Start.CompareTo(other.Start);
    }

    public bool Overlaps(Interval other)
    {
        return Start < other.End && End > other.Start;
    }
}