using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Trees;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for KdTreeTests.
/// </summary>
public class KdTreeTests
{
        /// <summary>
    /// RangeSearch_Returns_Points_In_Range method.
    /// </summary>
[Test]
    public void RangeSearch_Returns_Points_In_Range()
    {
        // Arrange
        var kdTree = new KdTree();
        kdTree.Insert(new Point(3, 6));
        kdTree.Insert(new Point(17, 15));
        kdTree.Insert(new Point(13, 15));
        kdTree.Insert(new Point(6, 12));

        var range = new Rect(5, 10, 15, 16);
            
        // Act
        var inRange = kdTree.RangeSearch(range).ToList();

        // Assert
        inRange.Count().ShouldBe(2);
        inRange.ShouldContain(p => p.X == 13 && p.Y == 15);
        inRange.ShouldContain(p => p.X == 6 && p.Y == 12);
    }
        
        /// <summary>
    /// NearestNeighbor_Returns_Closest_Point method.
    /// </summary>
[Test]
    public void NearestNeighbor_Returns_Closest_Point()
    {
        // Arrange
        var kdTree = new KdTree();
        kdTree.Insert(new Point(3, 6));
        kdTree.Insert(new Point(17, 15));
        kdTree.Insert(new Point(13, 15));
        kdTree.Insert(new Point(6, 12));
            
        // Act
        var nearest = kdTree.NearestNeighbor(new Point(5, 11));

        // Assert
        nearest.X.ShouldBe(6);
        nearest.Y.ShouldBe(12);
}
}