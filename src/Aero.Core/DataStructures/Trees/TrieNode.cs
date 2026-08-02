namespace Aero.Core.DataStructures.Trees;

/// <summary>
/// Represents a node in a Trie.
/// </summary>
public class TrieNode
{
        /// <summary>
    /// Gets or sets the Children.
    /// </summary>
public Dictionary<char, TrieNode> Children { get; } = new();
        /// <summary>
    /// Gets or sets the Is End Of Word.
    /// </summary>
public bool IsEndOfWord { get; set; }
}