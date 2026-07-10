using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;
using Humanizer;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for DirectedGraphTests.
/// </summary>
public class DirectedGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Vertex Tests

        /// <summary>
    /// AddVertex_ShouldIncreaseVertexCount method.
    /// </summary>
[Test]
    public void AddVertex_ShouldIncreaseVertexCount()
    {
        var graph = new DirectedGraph<string>();
        var vertex = _faker.Internet.UserName();

        graph.AddVertex(vertex);

        graph.VertexCount.ShouldBe(1);
    }

        /// <summary>
    /// AddVertex_ShouldReturnTrue_WhenVertexIsNew method.
    /// </summary>
[Test]
    public void AddVertex_ShouldReturnTrue_WhenVertexIsNew()
    {
        var graph = new DirectedGraph<int>();
        var vertex = _fixture.Create<int>();

        var result = graph.AddVertex(vertex);

        result.ShouldBeTrue();
    }

        /// <summary>
    /// AddVertex_ShouldReturnFalse_WhenAlreadyExists method.
    /// </summary>
[Test]
    public void AddVertex_ShouldReturnFalse_WhenAlreadyExists()
    {
        var graph = new DirectedGraph<string>();
        var vertex = _faker.Name.FirstName();
        graph.AddVertex(vertex);

        var result = graph.AddVertex(vertex);

        result.ShouldBeFalse();
    }

        /// <summary>
    /// AddVertex_ShouldInitializeEmptyEdgeLists method.
    /// </summary>
[Test]
    public void AddVertex_ShouldInitializeEmptyEdgeLists()
    {
        var graph = new DirectedGraph<string>();
        var vertex = "test_vertex".Humanize();

        graph.AddVertex(vertex);

        graph.GetOutDegree(vertex).ShouldBe(0);
        graph.GetInDegree(vertex).ShouldBe(0);
    }

    //#endregion

    //#region Edge Tests

        /// <summary>
    /// AddEdge_ShouldIncreaseEdgeCount method.
    /// </summary>
[Test]
    public void AddEdge_ShouldIncreaseEdgeCount()
    {
        var graph = new DirectedGraph<string>();
        var source = "follower".Humanize();
        var target = "following".Humanize();

        graph.AddEdge(source, target);

        graph.EdgeCount.ShouldBe(1);
    }

        /// <summary>
    /// AddEdge_ShouldCreateOneWayConnection method.
    /// </summary>
[Test]
    public void AddEdge_ShouldCreateOneWayConnection()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");

        graph.ContainsEdge("A", "B").ShouldBeTrue();
        graph.ContainsEdge("B", "A").ShouldBeFalse();
    }

        /// <summary>
    /// AddEdge_ShouldAutoAddVertices method.
    /// </summary>
[Test]
    public void AddEdge_ShouldAutoAddVertices()
    {
        var graph = new DirectedGraph<int>();
        var v1 = _fixture.Create<int>();
        var v2 = _fixture.Create<int>();

        graph.AddEdge(v1, v2);

        graph.VertexCount.ShouldBe(2);
    }

        /// <summary>
    /// AddEdge_ShouldAllowReverseEdge method.
    /// </summary>
[Test]
    public void AddEdge_ShouldAllowReverseEdge()
    {
        var graph = new DirectedGraph<string>();
        
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "A");

        graph.EdgeCount.ShouldBe(2);
        graph.ContainsEdge("A", "B").ShouldBeTrue();
        graph.ContainsEdge("B", "A").ShouldBeTrue();
    }

        /// <summary>
    /// AddEdge_ShouldNotDuplicate method.
    /// </summary>
[Test]
    public void AddEdge_ShouldNotDuplicate()
    {
        var graph = new DirectedGraph<string>();
        
        graph.AddEdge("X", "Y");
        graph.AddEdge("X", "Y");

        graph.EdgeCount.ShouldBe(1);
    }

    //#endregion

    //#region Degree Tests

        /// <summary>
    /// GetOutDegree_ShouldReturnCorrectValue method.
    /// </summary>
[Test]
    public void GetOutDegree_ShouldReturnCorrectValue()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("center", "a");
        graph.AddEdge("center", "b");
        graph.AddEdge("center", "c");

        graph.GetOutDegree("center").ShouldBe(3);
    }

        /// <summary>
    /// GetInDegree_ShouldReturnCorrectValue method.
    /// </summary>
[Test]
    public void GetInDegree_ShouldReturnCorrectValue()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("a", "center");
        graph.AddEdge("b", "center");
        graph.AddEdge("c", "center");

        graph.GetInDegree("center").ShouldBe(3);
    }

        /// <summary>
    /// Degree_ShouldHandleBothInAndOut method.
    /// </summary>
[Test]
    public void Degree_ShouldHandleBothInAndOut()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("x", "y");
        graph.AddEdge("y", "z");

        graph.GetInDegree("y").ShouldBe(1);
        graph.GetOutDegree("y").ShouldBe(1);
    }

    //#endregion

    //#region Neighbor Tests

        /// <summary>
    /// GetOutNeighbors_ShouldReturnCorrectVertices method.
    /// </summary>
[Test]
    public void GetOutNeighbors_ShouldReturnCorrectVertices()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");

        var neighbors = graph.GetOutNeighbors("A");

        neighbors.ShouldBe(new[] { "B", "C" }, ignoreOrder: true);
    }

        /// <summary>
    /// GetInNeighbors_ShouldReturnCorrectVertices method.
    /// </summary>
[Test]
    public void GetInNeighbors_ShouldReturnCorrectVertices()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("B", "A");
        graph.AddEdge("C", "A");

        var neighbors = graph.GetInNeighbors("A");

        neighbors.ShouldBe(new[] { "B", "C" }, ignoreOrder: true);
    }

    //#endregion

    //#region Remove Tests

        /// <summary>
    /// RemoveVertex_ShouldRemoveOutgoingEdges method.
    /// </summary>
[Test]
    public void RemoveVertex_ShouldRemoveOutgoingEdges()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("remove", "keep");
        graph.AddEdge("keep", "other");

        graph.RemoveVertex("remove");

        graph.ContainsEdge("remove", "keep").ShouldBeFalse();
        graph.ContainsVertex("keep").ShouldBeTrue();
    }

        /// <summary>
    /// RemoveVertex_ShouldRemoveIncomingEdges method.
    /// </summary>
[Test]
    public void RemoveVertex_ShouldRemoveIncomingEdges()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("keep", "remove");

        graph.RemoveVertex("remove");

        graph.ContainsEdge("keep", "remove").ShouldBeFalse();
    }

        /// <summary>
    /// RemoveEdge_ShouldOnlyRemoveSpecifiedDirection method.
    /// </summary>
[Test]
    public void RemoveEdge_ShouldOnlyRemoveSpecifiedDirection()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "A");

        graph.RemoveEdge("A", "B");

        graph.ContainsEdge("A", "B").ShouldBeFalse();
        graph.ContainsEdge("B", "A").ShouldBeTrue();
    }

    //#endregion

    //#region Traversal Tests

        /// <summary>
    /// BreadthFirstSearch_ShouldFollowOutgoingEdges method.
    /// </summary>
[Test]
    public void BreadthFirstSearch_ShouldFollowOutgoingEdges()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");

        var result = graph.BreadthFirstSearch("A").ToList();

        result.First().ShouldBe("A");
        result.ShouldBe(new[] { "B", "C", "D", "A" }, ignoreOrder: true);
    }

        /// <summary>
    /// DepthFirstSearch_ShouldFollowOutgoingEdges method.
    /// </summary>
[Test]
    public void DepthFirstSearch_ShouldFollowOutgoingEdges()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("1", "2");
        graph.AddEdge("1", "3");
        graph.AddEdge("2", "4");

        var result = graph.DepthFirstSearch("1").ToList();

        result.First().ShouldBe("1");
        result.Count().ShouldBe(4);
    }

        /// <summary>
    /// Traversal_ShouldNotFollowIncomingEdges method.
    /// </summary>
[Test]
    public void Traversal_ShouldNotFollowIncomingEdges()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("B", "A");
        graph.AddEdge("C", "A");

        var result = graph.BreadthFirstSearch("A").ToList();

        result.ShouldHaveSingleItem().ShouldBe("A");
    }

    //#endregion

    //#region Topological Sort Tests

        /// <summary>
    /// TopologicalSort_ShouldReturnValidOrder method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldReturnValidOrder()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("compile", "test");
        graph.AddEdge("test", "deploy");

        var result = graph.TopologicalSort();

        result.IndexOf("compile").ShouldBeLessThan(result.IndexOf("test"));
        result.IndexOf("test").ShouldBeLessThan(result.IndexOf("deploy"));
    }

        /// <summary>
    /// TopologicalSort_ShouldReturnEmpty_WhenCycleExists method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldReturnEmpty_WhenCycleExists()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        var result = graph.TopologicalSort();

        result.ShouldBeEmpty();
    }

        /// <summary>
    /// TopologicalSort_ShouldHandleDiamondDependency method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldHandleDiamondDependency()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "D");

        var result = graph.TopologicalSort();

        result.IndexOf("A").ShouldBeLessThan(result.IndexOf("B"));
        result.IndexOf("A").ShouldBeLessThan(result.IndexOf("C"));
        result.IndexOf("B").ShouldBeLessThan(result.IndexOf("D"));
        result.IndexOf("C").ShouldBeLessThan(result.IndexOf("D"));
    }

    //#endregion

    //#region Cycle Detection Tests

        /// <summary>
    /// HasCycle_ShouldReturnTrue_WhenCycleExists method.
    /// </summary>
[Test]
    public void HasCycle_ShouldReturnTrue_WhenCycleExists()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        graph.HasCycle().ShouldBeTrue();
    }

        /// <summary>
    /// HasCycle_ShouldReturnFalse_WhenNoCycle method.
    /// </summary>
[Test]
    public void HasCycle_ShouldReturnFalse_WhenNoCycle()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        graph.HasCycle().ShouldBeFalse();
    }

        /// <summary>
    /// HasCycle_ShouldDetectSelfLoop method.
    /// </summary>
[Test]
    public void HasCycle_ShouldDetectSelfLoop()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "A");

        graph.HasCycle().ShouldBeTrue();
    }

    //#endregion

    //#region Reachability Tests

        /// <summary>
    /// GetReachableVertices_ShouldReturnAllReachable method.
    /// </summary>
[Test]
    public void GetReachableVertices_ShouldReturnAllReachable()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");
        graph.AddVertex("E");

        var reachable = graph.GetReachableVertices("A");

        reachable.ShouldBe(new[] { "A", "B", "C", "D" }, ignoreOrder: true);
        reachable.ShouldNotContain("E");
    }

    //#endregion

    //#region Strongly Connected Components Tests

        /// <summary>
    /// GetStronglyConnectedComponents_ShouldIdentifyScc method.
    /// </summary>
[Test]
    public void GetStronglyConnectedComponents_ShouldIdentifyScc()
    {
        var graph = new DirectedGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");
        graph.AddVertex("D");

        var sccs = graph.GetStronglyConnectedComponents().ToList();

        sccs.Count().ShouldBe(2);
        sccs.Single(c => c.Contains("A") && c.Contains("B") && c.Contains("C")).ShouldNotBeNull();
        sccs.Single(c => c.Contains("D")).ShouldNotBeNull();
    }

    //#endregion

    //#region Clear Tests

        /// <summary>
    /// Clear_ShouldResetGraph method.
    /// </summary>
[Test]
    public void Clear_ShouldResetGraph()
    {
        var graph = new DirectedGraph<int>();
        var vertices = _fixture.CreateMany<int>(10);
        foreach (var v in vertices)
        {
            graph.AddVertex(v);
        }

        graph.Clear();

        graph.VertexCount.ShouldBe(0);
        graph.EdgeCount.ShouldBe(0);
}

    //#endregion
}