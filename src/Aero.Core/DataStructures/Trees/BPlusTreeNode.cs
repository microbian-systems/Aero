namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a B+ Tree.
/// </summary>
/// <typeparam name="T">The type of the keys in the B+ Tree.</typeparam>
public class BPlusTreeNode<T>(int degree)
{
        /// <summary>
    /// Gets or sets the Keys.
    /// </summary>
public List<T> Keys { get; } = new(degree);
        /// <summary>
    /// Gets or sets the Children.
    /// </summary>
public List<BPlusTreeNode<T>> Children { get; } = new(degree + 1);
        /// <summary>
    /// Gets or sets the Is Leaf.
    /// </summary>
public bool IsLeaf { get; set; }
        /// <summary>
    /// Gets or sets the Next.
    /// </summary>
public BPlusTreeNode<T> Next { get; set; } // For leaf nodes
}