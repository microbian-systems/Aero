namespace Aero.DataStructures.Trees;

/// <summary>
/// Represents a node in an Interval Tree.
/// </summary>
public class IntervalTreeNode(Interval interval)
{
    public Interval Interval { get; } = interval;
    public int Max { get; set; } = interval.End;
    public IntervalTreeNode Left { get; set; }
    public IntervalTreeNode Right { get; set; }
    public int Height { get; set; } = 1;
}