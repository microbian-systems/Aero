using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Trees;
using Bogus;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for BinaryTreeTests.
/// </summary>
public class BinaryTreeTests
{
    private readonly Faker _faker = new();

        /// <summary>
    /// Insert_SingleValue_RootIsCorrect method.
    /// </summary>
[Test]
    public void Insert_SingleValue_RootIsCorrect()
    {
        // Arrange
        var tree = new BinaryTree<int>();
        int value = _faker.Random.Int();

        // Act
        tree.Insert(value);

        // Assert
        tree.Root.ShouldNotBeNull();
        tree.Root.Value.ShouldBe(value);
}
}