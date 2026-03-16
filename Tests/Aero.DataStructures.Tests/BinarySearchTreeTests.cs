using Shouldly;
using Aero.DataStructures.Trees;
using Bogus;

namespace Aero.DataStructures.Tests;

public class BinarySearchTreeTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Insert_ShouldAddItemsCorrectly()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        var values = _faker.Random.Digits(10).Distinct().ToList();

        // Act
        foreach (var value in values)
        {
            bst.Insert(value);
        }

        // Assert
        foreach (var value in values)
        {
            bst.Find(value).ShouldNotBeNull();
            bst.Find(value).Value.ShouldBe(value);
        }
    }

    [Fact]
    public void Find_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);
        bst.Insert(15);

        // Act
        var result = bst.Find(999);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveLeafNode()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5); // Leaf
        bst.Insert(15);

        // Act
        bst.Delete(5);

        // Assert
        bst.Find(5).ShouldBeNull();
        bst.Root.Left.ShouldBeNull();
        bst.Root.Value.ShouldBe(10);
        bst.Root.Right.Value.ShouldBe(15);
    }

    [Fact]
    public void Delete_ShouldRemoveNodeWithOneChild_Left()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);
        bst.Insert(3); // 5 has left child 3

        // Act
        bst.Delete(5);

        // Assert
        bst.Find(5).ShouldBeNull();
        bst.Find(3).ShouldNotBeNull();
        bst.Root.Left.Value.ShouldBe(3);
    }

    [Fact]
    public void Delete_ShouldRemoveNodeWithOneChild_Right()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);
        bst.Insert(7); // 5 has right child 7

        // Act
        bst.Delete(5);

        // Assert
        bst.Find(5).ShouldBeNull();
        bst.Find(7).ShouldNotBeNull();
        bst.Root.Left.Value.ShouldBe(7);
    }

    [Fact]
    public void Delete_ShouldRemoveNodeWithTwoChildren()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);
        bst.Insert(3);
        bst.Insert(7); // 5 has children 3 and 7

        // Act
        bst.Delete(5);

        // Assert
        bst.Find(5).ShouldBeNull();
        // The successor of 5 (two children) should be the min value of right subtree (7).
        // Or if standard implementation, replaces value.
        bst.Root.Left.Value.ShouldBe(7); 
        bst.Find(3).ShouldNotBeNull();
        bst.Find(7).ShouldNotBeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveRoot_WhenRootIsLeaf()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);

        // Act
        bst.Delete(10);

        // Assert
        bst.Find(10).ShouldBeNull();
        bst.Root.ShouldBeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveRoot_WhenRootHasOneChild()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(15);

        // Act
        bst.Delete(10);

        // Assert
        bst.Find(10).ShouldBeNull();
        bst.Root.Value.ShouldBe(15);
    }

    [Fact]
    public void Delete_ShouldRemoveRoot_WhenRootHasTwoChildren()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);
        bst.Insert(15);
        bst.Insert(12);
        bst.Insert(20);

        // Act
        bst.Delete(10);

        // Assert
        bst.Find(10).ShouldBeNull();
        // Successor of 10 is min of right subtree (15's subtree), which is 12.
        bst.Root.Value.ShouldBe(12);
        bst.Root.Right.Value.ShouldBe(15);
        bst.Root.Left.Value.ShouldBe(5);
    }

    [Fact]
    public void Delete_ShouldDoNothing_WhenNodeNotFound()
    {
        // Arrange
        var bst = new BinarySearchTree<int>();
        bst.Insert(10);
        bst.Insert(5);

        // Act
        bst.Delete(99);

        // Assert
        bst.Find(10).ShouldNotBeNull();
        bst.Find(5).ShouldNotBeNull();
        bst.Root.Value.ShouldBe(10);
    }

    [Fact]
    public void Find_ShouldWorkWithStrings()
    {
        // Arrange
        var bst = new BinarySearchTree<string>();
        bst.Insert("apple");
        bst.Insert("banana");
        bst.Insert("cherry");

        // Act
        var node = bst.Find("banana");

        // Assert
        node.ShouldNotBeNull();
        node.Value.ShouldBe("banana");
    }
}