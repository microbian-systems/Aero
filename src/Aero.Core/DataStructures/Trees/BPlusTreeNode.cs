namespace Aero.DataStructures.Trees;

/// <summary>
/// Represents a node in a B+ Tree.
/// </summary>
/// <typeparam name="T">The type of the keys in the B+ Tree.</typeparam>
public class BPlusTreeNode<T>(int degree)
{
    public List<T> Keys { get; } = new(degree);
    public List<BPlusTreeNode<T>> Children { get; } = new(degree + 1);
    public bool IsLeaf { get; set; }
    public BPlusTreeNode<T> Next { get; set; } // For leaf nodes
}