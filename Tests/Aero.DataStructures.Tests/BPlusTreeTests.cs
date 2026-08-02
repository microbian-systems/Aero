using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Trees;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for BPlusTreeTests.
/// </summary>
public class BPlusTreeTests
{
        /// <summary>
    /// Insert_And_Find_Success method.
    /// </summary>
[Test]
    public void Insert_And_Find_Success()
    {
        // Arrange
        var bptree = new BPlusTree<int>(3);
        bptree.Insert(10);
        bptree.Insert(20);
        bptree.Insert(30);
        bptree.Insert(40);
        bptree.Insert(50);

        // Act
        var found = bptree.Find(30);

        // Assert
        found.Value.ShouldBe(30);
    }

        /// <summary>
    /// FindRange_Returns_Correct_Range method.
    /// </summary>
[Test]
    public void FindRange_Returns_Correct_Range()
    {
        // Arrange
        var bptree = new BPlusTree<int>(3);
        bptree.Insert(10);
        bptree.Insert(20);
        bptree.Insert(30);
        bptree.Insert(40);
        bptree.Insert(50);

        // Act
        var range = bptree.FindRange(20, 40).ToList();

        // Assert
        range.Count().ShouldBe(3);
        range.ShouldBeInOrder();
}
}