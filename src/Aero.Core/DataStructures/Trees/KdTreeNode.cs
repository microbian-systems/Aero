namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a KD-Tree.
/// </summary>
public class KdTreeNode(Point point)
{
    public Point Point { get; } = point;
    public KdTreeNode Left { get; set; }
    public KdTreeNode Right { get; set; }
}