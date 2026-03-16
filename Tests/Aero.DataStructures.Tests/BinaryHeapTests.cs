using Shouldly;
using Aero.DataStructures.Trees;

namespace Aero.DataStructures.Tests;

public class BinaryHeapTests
{
    [Fact]
    public void MinHeap_Extract_Returns_Minimum()
    {
        // Arrange
        var heap = new BinaryHeap<int>(HeapType.MinHeap);
        heap.Insert(5);
        heap.Insert(3);
        heap.Insert(8);
        heap.Insert(1);

        // Act
        var min = heap.Extract();

        // Assert
        min.ShouldBe(1);
        heap.Peek().ShouldBe(3);
    }

    [Fact]
    public void MaxHeap_Extract_Returns_Maximum()
    {
        // Arrange
        var heap = new BinaryHeap<int>(HeapType.MaxHeap);
        heap.Insert(5);
        heap.Insert(3);
        heap.Insert(8);
        heap.Insert(1);

        // Act
        var max = heap.Extract();

        // Assert
        max.ShouldBe(8);
        heap.Peek().ShouldBe(5);
    }
}