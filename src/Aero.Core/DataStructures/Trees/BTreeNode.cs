namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a B-Tree.
/// </summary>
/// <typeparam name="T">The type of the keys in the B-Tree.</typeparam>
public class BTreeNode<T>(int degree)
{
    public List<T> Keys { get; } = new(degree - 1);
    public List<BTreeNode<T>> Children { get; } = new(degree);
    public bool IsLeaf => Children.Count == 0;
}