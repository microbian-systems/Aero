using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;

namespace Aero.DataStructures.Tests;

public class DirectedAcyclicGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Vertex Tests

    [Fact]
    public void AddVertex_ShouldIncreaseCount()
    {
        var dag = new DirectedAcyclicGraph<string>();
        var vertex = _faker.Hacker.Noun();

        dag.AddVertex(vertex);

        dag.VertexCount.ShouldBe(1);
    }

    [Fact]
    public void AddVertex_ShouldReturnTrue_WhenNew()
    {
        var dag = new DirectedAcyclicGraph<int>();
        var vertex = _fixture.Create<int>();

        var result = dag.AddVertex(vertex);

        result.ShouldBeTrue();
    }

    [Fact]
    public void AddVertex_ShouldReturnFalse_WhenExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("existing");

        var result = dag.AddVertex("existing");

        result.ShouldBeFalse();
    }

    //#endregion

    //#region Edge Tests

    [Fact]
    public void AddEdge_ShouldAddSuccessfully_WhenNoCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        var act = () => dag.AddEdge("A", "B");

        act.ShouldNotThrow();
        dag.EdgeCount.ShouldBe(1);
    }

    [Fact]
    public void AddEdge_ShouldThrow_WhenWouldCreateCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var act = () => dag.AddEdge("C", "A");

        act.ShouldThrow<InvalidOperationException>("*cycle*");
    }

    [Fact]
    public void AddEdge_ShouldAutoAddVertices()
    {
        var dag = new DirectedAcyclicGraph<int>();
        var v1 = _fixture.Create<int>();
        var v2 = _fixture.Create<int>();

        dag.AddEdge(v1, v2);

        dag.ContainsVertex(v1).ShouldBeTrue();
        dag.ContainsVertex(v2).ShouldBeTrue();
    }

    [Fact]
    public void TryAddEdge_ShouldReturnTrue_WhenNoCycle()
    {
        var dag = new DirectedAcyclicGraph<string>();
        
        var result = dag.TryAddEdge("A", "B");

        result.ShouldBeTrue();
    }

    [Fact]
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

    [Fact]
    public void WouldCreateCycle_ShouldReturnTrue_WhenPathExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        dag.WouldCreateCycle("C", "A").ShouldBeTrue();
    }

    [Fact]
    public void WouldCreateCycle_ShouldReturnFalse_WhenNoPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        dag.WouldCreateCycle("A", "B").ShouldBeFalse();
    }

    //#endregion

    //#region Reachability Tests

    [Fact]
    public void CanReach_ShouldReturnTrue_WhenPathExists()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        dag.CanReach("A", "C").ShouldBeTrue();
    }

    [Fact]
    public void CanReach_ShouldReturnFalse_WhenNoPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");

        dag.CanReach("A", "B").ShouldBeFalse();
    }

    [Fact]
    public void CanReach_ShouldReturnTrue_ForSameVertex()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");

        dag.CanReach("A", "A").ShouldBeTrue();
    }

    //#endregion

    //#region Topological Sort Tests

    [Fact]
    public void TopologicalSort_ShouldReturnAllVertices()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        var result = dag.TopologicalSort();

        result.Count().ShouldBe(3);
        result.ShouldContain(new[] { "A", "B", "C" });
    }

    [Fact]
    public void TopologicalSort_ShouldRespectDependencies()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("compile", "test");
        dag.AddEdge("test", "deploy");

        var result = dag.TopologicalSort();

        result.IndexOf("compile").ShouldBeLessThan(result.IndexOf("test"));
        result.IndexOf("test").ShouldBeLessThan(result.IndexOf("deploy"));
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public void GetSources_ShouldReturnVerticesWithNoIncoming()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("root", "child1");
        dag.AddEdge("root", "child2");

        var sources = dag.GetSources().ToList();

        sources.ShouldContainSingle("root");
    }

    [Fact]
    public void GetSinks_ShouldReturnVerticesWithNoOutgoing()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("parent", "leaf1");
        dag.AddEdge("parent", "leaf2");

        var sinks = dag.GetSinks().ToList();

        sinks.ShouldContain(new[] { "leaf1", "leaf2" });
    }

    //#endregion

    //#region Longest Path Tests

    [Fact]
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

    [Fact]
    public void GetLongestPath_ShouldReturnCorrectPath()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("A", "C");

        var path = dag.GetLongestPath();

        path.ShouldContainInOrder("A", "B", "C");
    }

    //#endregion

    //#region Ancestor/Descendant Tests

    [Fact]
    public void GetAncestors_ShouldReturnAllPredecessors()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("X", "C");

        var ancestors = dag.GetAncestors("C");

        ancestors.ShouldContain(new[] { "A", "B", "X" });
    }

    [Fact]
    public void GetDescendants_ShouldReturnAllSuccessors()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("A", "D");

        var descendants = dag.GetDescendants("A");

        descendants.ShouldContain(new[] { "B", "C", "D" });
    }

    //#endregion

    //#region LCA Tests

    [Fact]
    public void GetLowestCommonAncestors_ShouldFindCorrectLca()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddEdge("root", "left");
        dag.AddEdge("root", "right");
        dag.AddEdge("left", "target");
        dag.AddEdge("right", "target");

        var lcas = dag.GetLowestCommonAncestors("left", "right");

        lcas.ShouldContainSingle("root");
    }

    //#endregion

    //#region Transitive Closure Tests

    [Fact]
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

    [Fact]
    public void RemoveVertex_ShouldRemoveFromDag()
    {
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("remove");
        dag.AddVertex("keep");

        dag.RemoveVertex("remove");

        dag.ContainsVertex("remove").ShouldBeFalse();
        dag.VertexCount.ShouldBe(1);
    }

    [Fact]
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
