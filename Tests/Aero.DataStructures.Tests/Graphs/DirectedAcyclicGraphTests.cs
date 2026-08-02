using TUnit.Core;
using Shouldly;
using Aero.DataStructures;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for DirectedAcyclicGraphTests.
/// </summary>
public class DirectedAcyclicGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Vertex Tests

        /// <summary>
    /// AddVertex_ShouldIncreaseCount method.
    /// </summary>
[Test]
    public void AddVertex_ShouldIncreaseCount()
    {
        var dag = new DirectedAcyclicGraph<string>();
        var vertex = _faker.Hacker.Noun();

        dag.AddVertex(vertex);

        dag.VertexCount.ShouldBe(1);
    }

        /// <summary>
    /// AddVertex_ShouldReturnTrue_WhenNew method.
    /// </summary>
[Test]
    public void AddVertex_ShouldReturnTrue_WhenNew()
    {
        var dag = new DirectedAcyclicGraph<int>();
        var vertex = _fixture.Create<int>();

        var result = dag.AddVertex(vertex);

        result.ShouldBeTrue();
    }

        /// <summary>
    /// AddVertex_ShouldReturnFalse_WhenExists method.
    /// </summary>
[Test]
    public void AddVertex_ShouldReturnFalse_WhenExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("existing");

        var result = dag.AddVertex("existing");

        result.ShouldBeFalse();
    }

    //#endregion

    //#region Edge Tests

        /// <summary>
    /// AddEdge_ShouldAddSuccessfully_WhenNoCycle method.
    /// </summary>
[Test]
    public void AddEdge_ShouldAddSuccessfully_WhenNoCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        var act = () => dag.AddEdge("A", "B");

        act.ShouldNotThrow();
        dag.EdgeCount.ShouldBe(1);
    }

        /// <summary>
    /// AddEdge_ShouldThrow_WhenWouldCreateCycle method.
    /// </summary>
[Test]
    public void AddEdge_ShouldThrow_WhenWouldCreateCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var act = () => dag.AddEdge("C", "A");

        act.ShouldThrow<InvalidOperationException>("*cycle*");
    }

        /// <summary>
    /// AddEdge_ShouldAutoAddVertices method.
    /// </summary>
[Test]
    public void AddEdge_ShouldAutoAddVertices()
    {
        var dag = new DirectedAcyclicGraph<int>();
        var v1 = _fixture.Create<int>();
        var v2 = _fixture.Create<int>();

        dag.AddEdge(v1, v2);

        dag.ContainsVertex(v1).ShouldBeTrue();
        dag.ContainsVertex(v2).ShouldBeTrue();
    }

        /// <summary>
    /// TryAddEdge_ShouldReturnTrue_WhenNoCycle method.
    /// </summary>
[Test]
    public void TryAddEdge_ShouldReturnTrue_WhenNoCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        
        var result = dag.TryAddEdge("A", "B");

        result.ShouldBeTrue();
    }

        /// <summary>
    /// TryAddEdge_ShouldReturnFalse_WhenWouldCreateCycle method.
    /// </summary>
[Test]
    public void TryAddEdge_ShouldReturnFalse_WhenWouldCreateCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var result = dag.TryAddEdge("C", "A");

        result.ShouldBeFalse();
    }

    //#endregion

    //#region WouldCreateCycle Tests

        /// <summary>
    /// WouldCreateCycle_ShouldReturnTrue_WhenPathExists method.
    /// </summary>
[Test]
    public void WouldCreateCycle_ShouldReturnTrue_WhenPathExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        dag.WouldCreateCycle("C", "A").ShouldBeTrue();
    }

        /// <summary>
    /// WouldCreateCycle_ShouldReturnFalse_WhenNoPath method.
    /// </summary>
[Test]
    public void WouldCreateCycle_ShouldReturnFalse_WhenNoPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        dag.WouldCreateCycle("A", "B").ShouldBeFalse();
    }

    //#endregion

    //#region Reachability Tests

        /// <summary>
    /// CanReach_ShouldReturnTrue_WhenPathExists method.
    /// </summary>
[Test]
    public void CanReach_ShouldReturnTrue_WhenPathExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        dag.CanReach("A", "C").ShouldBeTrue();
    }

        /// <summary>
    /// CanReach_ShouldReturnFalse_WhenNoPath method.
    /// </summary>
[Test]
    public void CanReach_ShouldReturnFalse_WhenNoPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        dag.CanReach("A", "B").ShouldBeFalse();
    }

        /// <summary>
    /// CanReach_ShouldReturnTrue_ForSameVertex method.
    /// </summary>
[Test]
    public void CanReach_ShouldReturnTrue_ForSameVertex()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");

        dag.CanReach("A", "A").ShouldBeTrue();
    }

    //#endregion

    //#region Topological Sort Tests

        /// <summary>
    /// TopologicalSort_ShouldReturnAllVertices method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldReturnAllVertices()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var result = dag.TopologicalSort();

        result.Count().ShouldBe(3);
        result.ShouldBe(new[] { "A", "B", "C" });
    }

        /// <summary>
    /// TopologicalSort_ShouldRespectDependencies method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldRespectDependencies()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("compile", "test");
        dag.AddEdge("test", "deploy");

        var result = dag.TopologicalSort();

        result.IndexOf("compile").ShouldBeLessThan(result.IndexOf("test"));
        result.IndexOf("test").ShouldBeLessThan(result.IndexOf("deploy"));
    }

        /// <summary>
    /// TopologicalSort_ShouldHandleMultipleSources method.
    /// </summary>
[Test]
    public void TopologicalSort_ShouldHandleMultipleSources()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "C");

        var result = dag.TopologicalSort();

        result.IndexOf("A").ShouldBeLessThan(result.IndexOf("C"));
        result.IndexOf("B").ShouldBeLessThan(result.IndexOf("C"));
    }

    //#endregion

    //#region All Topological Sorts Tests

        /// <summary>
    /// GetAllTopologicalSorts_ShouldReturnMultipleOrders method.
    /// </summary>
[Test]
    public void GetAllTopologicalSorts_ShouldReturnMultipleOrders()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");

        var allSorts = dag.GetAllTopologicalSorts().ToList();

        allSorts.Count().ShouldBe(6);
    }

    //#endregion

    //#region Sources and Sinks Tests

        /// <summary>
    /// GetSources_ShouldReturnVerticesWithNoIncoming method.
    /// </summary>
[Test]
    public void GetSources_ShouldReturnVerticesWithNoIncoming()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("root", "child1");
        dag.AddEdge("root", "child2");

        var sources = dag.GetSources().ToList();

        sources.ShouldHaveSingleItem().ShouldBe("root");
    }

        /// <summary>
    /// GetSinks_ShouldReturnVerticesWithNoOutgoing method.
    /// </summary>
[Test]
    public void GetSinks_ShouldReturnVerticesWithNoOutgoing()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("parent", "leaf1");
        dag.AddEdge("parent", "leaf2");

        var sinks = dag.GetSinks().ToList();

        sinks.ShouldBe(new[] { "leaf1", "leaf2" });
    }

    //#endregion

    //#region Longest Path Tests

        /// <summary>
    /// GetLongestPathLengths_ShouldComputeCorrectly method.
    /// </summary>
[Test]
    public void GetLongestPathLengths_ShouldComputeCorrectly()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("A", "D");
        dag.AddEdge("D", "C");

        var lengths = dag.GetLongestPathLengths();

        lengths["A"].ShouldBe(0);
        lengths["B"].ShouldBe(1);
        lengths["D"].ShouldBe(1);
        lengths["C"].ShouldBe(2);
    }

        /// <summary>
    /// GetLongestPath_ShouldReturnCorrectPath method.
    /// </summary>
[Test]
    public void GetLongestPath_ShouldReturnCorrectPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("A", "C");

        var path = dag.GetLongestPath();

        path.ShouldBe(new[] { "A", "B", "C" });
    }

    //#endregion

    //#region Ancestor/Descendant Tests

        /// <summary>
    /// GetAncestors_ShouldReturnAllPredecessors method.
    /// </summary>
[Test]
    public void GetAncestors_ShouldReturnAllPredecessors()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("X", "C");

        var ancestors = dag.GetAncestors("C");

        ancestors.OrderBy(x => x).ShouldBe(new[] { "A", "B", "X" }.OrderBy(x => x));
    }

        /// <summary>
    /// GetDescendants_ShouldReturnAllSuccessors method.
    /// </summary>
[Test]
    public void GetDescendants_ShouldReturnAllSuccessors()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("A", "D");

        var descendants = dag.GetDescendants("A");

        descendants.OrderBy(x => x).ShouldBe(new[] { "B", "C", "D" }.OrderBy(x => x));
    }

    //#endregion

    //#region LCA Tests

        /// <summary>
    /// GetLowestCommonAncestors_ShouldFindCorrectLca method.
    /// </summary>
[Test]
    public void GetLowestCommonAncestors_ShouldFindCorrectLca()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("root", "left");
        dag.AddEdge("root", "right");
        dag.AddEdge("left", "target");
        dag.AddEdge("right", "target");

        var lcas = dag.GetLowestCommonAncestors("left", "right");

        lcas.ShouldHaveSingleItem().ShouldBe("root");
    }

    //#endregion

    //#region Transitive Closure Tests

        /// <summary>
    /// GetTransitiveClosure_ShouldAddIndirectEdges method.
    /// </summary>
[Test]
    public void GetTransitiveClosure_ShouldAddIndirectEdges()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var closure = dag.GetTransitiveClosure();

        closure.ContainsEdge("A", "C").ShouldBeTrue();
    }

    //#endregion

    //#region Remove Tests

        /// <summary>
    /// RemoveVertex_ShouldRemoveFromDag method.
    /// </summary>
[Test]
    public void RemoveVertex_ShouldRemoveFromDag()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("remove");
        dag.AddVertex("keep");

        dag.RemoveVertex("remove");

        dag.ContainsVertex("remove").ShouldBeFalse();
        dag.VertexCount.ShouldBe(1);
    }

        /// <summary>
    /// RemoveEdge_ShouldAllowPreviousCycle method.
    /// </summary>
[Test]
    public void RemoveEdge_ShouldAllowPreviousCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        dag.RemoveEdge("B", "C");
        
        var act = () => dag.AddEdge("C", "A");
        act.ShouldNotThrow();
}

    //#endregion
}