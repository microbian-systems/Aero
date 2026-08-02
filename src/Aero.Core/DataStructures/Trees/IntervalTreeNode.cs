namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in an Interval Tree.
/// </summary>
public class IntervalTreeNode(Interval interval)
{
        /// <summary>
    /// Gets or sets the Interval.
    /// </summary>
public Interval Interval { get; } = interval;
        /// <summary>
    /// Gets or sets the Max.
    /// </summary>
public int Max { get; set; } = interval.End;
        /// <summary>
    /// Gets or sets the Left.
    /// </summary>
public IntervalTreeNode Left { get; set; }
        /// <summary>
    /// Gets or sets the Right.
    /// </summary>
public IntervalTreeNode Right { get; set; }
        /// <summary>
    /// Gets or sets the Height.
    /// </summary>
public int Height { get; set; } = 1;
}