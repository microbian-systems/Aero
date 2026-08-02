using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Trees;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for TrieTests.
/// </summary>
public class TrieTests
{
        /// <summary>
    /// Insert_And_Search_Success method.
    /// </summary>
[Test]
    public void Insert_And_Search_Success()
    {
        // Arrange
        var trie = new Trie();
        trie.Insert("apple");
        trie.Insert("app");
            
        // Assert
        trie.Search("app").ShouldBeTrue();
        trie.Search("apple").ShouldBeTrue();
        trie.Search("appl").ShouldBeFalse();
    }

        /// <summary>
    /// StartsWith_Success method.
    /// </summary>
[Test]
    public void StartsWith_Success()
    {
        // Arrange
        var trie = new Trie();
        trie.Insert("apple");
        trie.Insert("app");
        trie.Insert("banana");
            
        // Assert
        trie.StartsWith("ap").ShouldBeTrue();
        trie.StartsWith("ban").ShouldBeTrue();
        trie.StartsWith("can").ShouldBeFalse();
    }

        /// <summary>
    /// Delete_And_Search_Fails method.
    /// </summary>
[Test]
    public void Delete_And_Search_Fails()
    {
        // Arrange
        var trie = new Trie();
        trie.Insert("apple");
        trie.Insert("app");

        // Act
        trie.Delete("apple");

        // Assert
        trie.Search("apple").ShouldBeFalse();
        trie.Search("app").ShouldBeTrue();
}
}