namespace Aero.Core.DataStructures.Trees;



/// <summary>
/// Represents a node in an R-Tree.
/// </summary>
public class RTreeNode
{
        /// <summary>
    /// Gets or sets the Mbr.
    /// </summary>
public Mbr Mbr { get; set; }
        /// <summary>
    /// Gets or sets the Parent.
    /// </summary>
public RTreeNode Parent { get; set; }
        /// <summary>
    /// Gets or sets the Children.
    /// </summary>
public List<RTreeNode> Children { get; } = new();
        /// <summary>
    /// Gets or sets the Points.
    /// </summary>
public List<Point> Points { get; } = new();

        /// <summary>
    /// Gets or sets the Is Leaf.
    /// </summary>
public bool IsLeaf => Children.Count == 0;
}