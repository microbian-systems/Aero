namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a Segment Tree.
/// </summary>
public class SegmentTreeNode
{
        /// <summary>
    /// Gets or sets the Start.
    /// </summary>
public int Start { get; set; }
        /// <summary>
    /// Gets or sets the End.
    /// </summary>
public int End { get; set; }
        /// <summary>
    /// Gets or sets the Sum.
    /// </summary>
public int Sum { get; set; }
        /// <summary>
    /// Gets or sets the Left.
    /// </summary>
public SegmentTreeNode Left { get; set; }
        /// <summary>
    /// Gets or sets the Right.
    /// </summary>
public SegmentTreeNode Right { get; set; }
}