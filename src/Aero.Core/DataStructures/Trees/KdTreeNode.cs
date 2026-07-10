namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a KD-Tree.
/// </summary>
public class KdTreeNode(Point point)
{
        /// <summary>
    /// Gets or sets the Point.
    /// </summary>
public Point Point { get; } = point;
        /// <summary>
    /// Gets or sets the Left.
    /// </summary>
public KdTreeNode Left { get; set; }
        /// <summary>
    /// Gets or sets the Right.
    /// </summary>
public KdTreeNode Right { get; set; }
}