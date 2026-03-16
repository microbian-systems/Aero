using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;
using Humanizer;

namespace Aero.DataStructures.Tests;

public class BipartiteGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Partition Tests

    [Fact]
    public void AddVertexToSetU_ShouldIncreaseSetUCount()
    {
        var graph = new BipartiteGraph<string>();
        var vertex = _faker.Name.FirstName();

        graph.AddVertexToSetU(vertex);

        graph.SetUCount.ShouldBe(1);
        graph.SetU.ShouldContain(vertex);
    }

    [Fact]
    public void AddVertexToSetV_ShouldIncreaseSetVCount()
    {
        var graph = new BipartiteGraph<string>();
        var vertex = _faker.Company.CompanyName();

        graph.AddVertexToSetV(vertex);

        graph.SetVCount.ShouldBe(1);
        graph.SetV.ShouldContain(vertex);
    }

    [Fact]
    public void AddVertexToSetU_ShouldNotAllowDuplicate()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("worker");

        var result = graph.AddVertexToSetU("worker");

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddVertex_ShouldNotAllowSameInBothSets()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("same");

        var result = graph.AddVertexToSetV("same");

        result.ShouldBeFalse();
    }

    [Fact]
    public void GetPartition_ShouldReturnCorrectPartition()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("u_vertex");
        graph.AddVertexToSetV("v_vertex");

        graph.GetPartition("u_vertex").ShouldBe('U');
        graph.GetPartition("v_vertex").ShouldBe('V');
        graph.GetPartition("nonexistent").ShouldBeNull();
    }

    //#endregion

    //#region Edge Tests

    [Fact]
    public void AddEdge_ShouldConnectUToV()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("worker");
        graph.AddVertexToSetV("job");

        graph.AddEdge("worker", "job");

        graph.ContainsEdge("worker", "job").ShouldBeTrue();
        graph.EdgeCount.ShouldBe(1);
    }

    [Fact]
    public void AddEdge_ShouldConnectVToU()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("worker");
        graph.AddVertexToSetV("job");

        graph.AddEdge("job", "worker");

        graph.ContainsEdge("job", "worker").ShouldBeTrue();
    }

    [Fact]
    public void AddEdge_ShouldThrow_WhenBothInSamePartition()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("u1");
        graph.AddVertexToSetU("u2");

        var act = () => graph.AddEdge("u1", "u2");

        act.ShouldThrow<ArgumentException>("*same partition*");
    }

    [Fact]
    public void AddEdge_ShouldThrow_WhenVertexDoesNotExist()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("u1");

        var act = () => graph.AddEdge("u1", "nonexistent");

        act.ShouldThrow<ArgumentException>();
    }

    //#endregion

    //#region Neighbor Tests

    [Fact]
    public void GetNeighbors_ShouldReturnOppositePartition()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("worker");
        graph.AddVertexToSetV("job1");
        graph.AddVertexToSetV("job2");
        graph.AddEdge("worker", "job1");
        graph.AddEdge("worker", "job2");

        var neighbors = graph.GetNeighbors("worker");

        neighbors.ShouldContain(new[] { "job1", "job2" });
    }

    //#endregion

    //#region Matching Tests

    [Fact]
    public void FindMaximumMatching_ShouldFindPerfectMatching_WhenExists()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("w1");
        graph.AddVertexToSetU("w2");
        graph.AddVertexToSetV("j1");
        graph.AddVertexToSetV("j2");
        graph.AddEdge("w1", "j1");
        graph.AddEdge("w1", "j2");
        graph.AddEdge("w2", "j1");
        graph.AddEdge("w2", "j2");

        var matching = graph.FindMaximumMatching();

        matching.Count.ShouldBe(2);
    }

    [Fact]
    public void FindMaximumMatching_ShouldHandleUnbalancedGraph()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("w1");
        graph.AddVertexToSetU("w2");
        graph.AddVertexToSetV("j1");
        graph.AddEdge("w1", "j1");
        graph.AddEdge("w2", "j1");

        var matching = graph.FindMaximumMatching();

        matching.Count.ShouldBe(1);
    }

    [Fact]
    public void FindMaximumMatching_ShouldReturnEmpty_WhenNoEdges()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("isolated1");
        graph.AddVertexToSetV("isolated2");

        var matching = graph.FindMaximumMatching();

        matching.ShouldBeEmpty();
    }

    //#endregion

    //#region Perfect Matching Tests

    [Fact]
    public void HasPerfectMatching_ShouldReturnTrue_WhenExists()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("a");
        graph.AddVertexToSetU("b");
        graph.AddVertexToSetV("1");
        graph.AddVertexToSetV("2");
        graph.AddEdge("a", "1");
        graph.AddEdge("b", "2");

        graph.HasPerfectMatching().ShouldBeTrue();
    }

    [Fact]
    public void HasPerfectMatching_ShouldReturnFalse_WhenUnbalanced()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("a");
        graph.AddVertexToSetU("b");
        graph.AddVertexToSetV("1");
        graph.AddEdge("a", "1");
        graph.AddEdge("b", "1");

        graph.HasPerfectMatching().ShouldBeFalse();
    }

    [Fact]
    public void FindPerfectMatching_ShouldReturnNull_WhenNotExists()
    {
        var graph = new BipartiteGraph<int>();
        graph.AddVertexToSetU(1);
        graph.AddVertexToSetV(2);
        graph.AddVertexToSetV(3);

        var perfect = graph.FindPerfectMatching();

        perfect.ShouldBeNull();
    }

    //#endregion

    //#region Vertex Cover Tests

    [Fact]
    public void FindMinimumVertexCover_ShouldReturnCorrectSize()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("u1");
        graph.AddVertexToSetU("u2");
        graph.AddVertexToSetV("v1");
        graph.AddVertexToSetV("v2");
        graph.AddEdge("u1", "v1");
        graph.AddEdge("u1", "v2");
        graph.AddEdge("u2", "v1");

        var cover = graph.FindMinimumVertexCover();

        cover.Count.ShouldBe(graph.FindMaximumMatching().Count);
    }

    //#endregion

    //#region Remove Tests

    [Fact]
    public void RemoveVertex_ShouldRemoveFromCorrectSet()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("remove");
        graph.AddVertexToSetV("keep");

        graph.RemoveVertex("remove");

        graph.SetUCount.ShouldBe(0);
        graph.SetVCount.ShouldBe(1);
    }

    [Fact]
    public void RemoveEdge_ShouldDecreaseEdgeCount()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("u");
        graph.AddVertexToSetV("v");
        graph.AddEdge("u", "v");

        graph.RemoveEdge("u", "v");

        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Clear Tests

    [Fact]
    public void Clear_ShouldRemoveAllVerticesAndEdges()
    {
        var graph = new BipartiteGraph<string>();
        graph.AddVertexToSetU("worker1");
        graph.AddVertexToSetU("worker2");
        graph.AddVertexToSetV("job1");
        graph.AddVertexToSetV("job2");
        graph.AddEdge("worker1", "job1");

        graph.Clear();

        graph.VertexCount.ShouldBe(0);
        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Real-World Scenario Tests

    [Fact]
    public void JobAssignmentScenario_ShouldWorkCorrectly()
    {
        var graph = new BipartiteGraph<string>();
        
        var workers = new[] { "Alice".Humanize(), "Bob".Humanize(), "Charlie".Humanize() };
        var jobs = new[] { "frontend".Humanize(), "backend".Humanize(), "devops".Humanize() };

        foreach (var w in workers) graph.AddVertexToSetU(w);
        foreach (var j in jobs) graph.AddVertexToSetV(j);

        graph.AddEdge("Alice", "Frontend");
        graph.AddEdge("Alice", "Backend");
        graph.AddEdge("Bob", "Backend");
        graph.AddEdge("Bob", "Devops");
        graph.AddEdge("Charlie", "Frontend");
        graph.AddEdge("Charlie", "Devops");

        var matching = graph.FindMaximumMatching();

        matching.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    //#endregion
}
