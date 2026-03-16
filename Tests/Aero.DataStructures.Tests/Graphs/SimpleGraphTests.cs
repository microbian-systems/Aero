using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;
using Humanizer;

namespace Aero.DataStructures.Tests;

public class SimpleGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Constructor Tests

    [Fact]
    public void Constructor_ShouldCreateUndirectedGraph_ByDefault()
    {
        var graph = new SimpleGraph<int>();

        graph.IsDirected.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ShouldRespectDirectedParameter()
    {
        var graph = new SimpleGraph<string>(directed: true);

        graph.IsDirected.ShouldBeTrue();
    }

    //#endregion

    //#region Vertex Tests

    [Fact]
    public void AddVertex_ShouldIncreaseVertexCount()
    {
        var graph = new SimpleGraph<string>();
        var vertex = _faker.Internet.UserName();

        graph.AddVertex(vertex);

        graph.VertexCount.ShouldBe(1);
    }

    [Fact]
    public void AddVertex_ShouldReturnTrue_WhenNew()
    {
        var graph = new SimpleGraph<int>();
        var vertex = _fixture.Create<int>();

        var result = graph.AddVertex(vertex);

        result.ShouldBeTrue();
    }

    [Fact]
    public void AddVertex_ShouldReturnFalse_WhenExists()
    {
        var graph = new SimpleGraph<string>();
        graph.AddVertex("existing");

        var result = graph.AddVertex("existing");

        result.ShouldBeFalse();
    }

    [Fact]
    public void ContainsVertex_ShouldReturnCorrectResult()
    {
        var graph = new SimpleGraph<string>();
        graph.AddVertex("present");

        graph.ContainsVertex("present").ShouldBeTrue();
        graph.ContainsVertex("absent").ShouldBeFalse();
    }

    //#endregion

    //#region Edge Tests

    [Fact]
    public void AddEdge_ShouldIncreaseEdgeCount()
    {
        var graph = new SimpleGraph<string>();

        graph.AddEdge("A", "B");

        graph.EdgeCount.ShouldBe(1);
    }

    [Fact]
    public void AddEdge_ShouldAutoAddVertices()
    {
        var graph = new SimpleGraph<int>();
        var v1 = _fixture.Create<int>();
        var v2 = _fixture.Create<int>();

        graph.AddEdge(v1, v2);

        graph.ContainsVertex(v1).ShouldBeTrue();
        graph.ContainsVertex(v2).ShouldBeTrue();
    }

    [Fact]
    public void AddEdge_ShouldThrow_OnSelfLoop()
    {
        var graph = new SimpleGraph<string>();

        var act = () => graph.AddEdge("self", "self");

        act.ShouldThrow<ArgumentException>("*Self-loop*");
    }

    [Fact]
    public void AddEdge_ShouldReturnFalse_OnDuplicate()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");

        var result = graph.AddEdge("A", "B");

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddEdge_Undirected_ShouldBeSymmetric()
    {
        var graph = new SimpleGraph<string>(directed: false);
        graph.AddEdge("X", "Y");

        graph.ContainsEdge("X", "Y").ShouldBeTrue();
        graph.ContainsEdge("Y", "X").ShouldBeTrue();
    }

    [Fact]
    public void AddEdge_Directed_ShouldNotBeSymmetric()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("X", "Y");

        graph.ContainsEdge("X", "Y").ShouldBeTrue();
        graph.ContainsEdge("Y", "X").ShouldBeFalse();
    }

    [Fact]
    public void GetEdges_ShouldReturnAllEdges()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        var edges = graph.GetEdges().ToList();

        edges.Should().HaveCount(3);
    }

    //#endregion

    //#region Degree Tests

    [Fact]
    public void GetDegree_Undirected_ShouldCountNeighbors()
    {
        var graph = new SimpleGraph<string>(directed: false);
        graph.AddEdge("center", "n1");
        graph.AddEdge("center", "n2");
        graph.AddEdge("center", "n3");

        graph.GetDegree("center").ShouldBe(3);
    }

    [Fact]
    public void GetDegree_Directed_ShouldCountBothDirections()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("center", "out1");
        graph.AddEdge("center", "out2");
        graph.AddEdge("in1", "center");

        graph.GetDegree("center").ShouldBe(3);
    }

    [Fact]
    public void GetInDegree_ShouldCountIncoming()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("a", "target");
        graph.AddEdge("b", "target");
        graph.AddEdge("target", "c");

        graph.GetInDegree("target").ShouldBe(2);
    }

    [Fact]
    public void GetOutDegree_ShouldCountOutgoing()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("source", "a");
        graph.AddEdge("source", "b");
        graph.AddEdge("c", "source");

        graph.GetOutDegree("source").ShouldBe(2);
    }

    [Fact]
    public void GetInDegree_ShouldThrow_ForUndirected()
    {
        var graph = new SimpleGraph<string>(directed: false);
        graph.AddVertex("v");

        var act = () => graph.GetInDegree("v");

        act.ShouldThrow<InvalidOperationException>();
    }

    //#endregion

    //#region Neighbor Tests

    [Fact]
    public void GetNeighbors_ShouldReturnAdjacentVertices()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("center", "n1");
        graph.AddEdge("center", "n2");
        graph.AddEdge("center", "n3");

        var neighbors = graph.GetNeighbors("center");

        neighbors.ShouldContain(new[] { "n1", "n2", "n3" });
    }

    [Fact]
    public void GetVertices_ShouldReturnAllVertices()
    {
        var graph = new SimpleGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);

        graph.GetVertices().ShouldContain(new[] { 1, 2, 3 });
    }

    //#endregion

    //#region Traversal Tests

    [Fact]
    public void BreadthFirstSearch_ShouldVisitAllVertices()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");

        var result = graph.BreadthFirstSearch("A").ToList();

        result.Should().HaveCount(4);
        result.First().ShouldBe("A");
    }

    [Fact]
    public void DepthFirstSearch_ShouldVisitAllVertices()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("1", "2");
        graph.AddEdge("2", "3");
        graph.AddEdge("3", "4");

        var result = graph.DepthFirstSearch("1").ToList();

        result.Should().HaveCount(4);
        result.First().ShouldBe("1");
    }

    [Fact]
    public void Traversal_ShouldReturnEmpty_ForNonExistentVertex()
    {
        var graph = new SimpleGraph<string>();

        graph.BreadthFirstSearch("nonexistent").ShouldBeEmpty();
        graph.DepthFirstSearch("nonexistent").ShouldBeEmpty();
    }

    //#endregion

    //#region Shortest Path Tests

    [Fact]
    public void GetShortestPath_ShouldFindDirectPath()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");

        var path = graph.GetShortestPath("A", "B");

        path.ShouldContainInOrder("A", "B");
    }

    [Fact]
    public void GetShortestPath_ShouldFindMultiHopPath()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");

        var path = graph.GetShortestPath("A", "D");

        path.ShouldContainInOrder("A", "B", "C", "D");
        path.Count.ShouldBe(4);
    }

    [Fact]
    public void GetShortestPath_ShouldReturnEmpty_WhenNoPath()
    {
        var graph = new SimpleGraph<string>();
        graph.AddVertex("isolated");
        graph.AddVertex("separate");

        var path = graph.GetShortestPath("isolated", "separate");

        path.ShouldBeEmpty();
    }

    [Fact]
    public void GetShortestPath_ShouldReturnSelf_WhenSourceIsTarget()
    {
        var graph = new SimpleGraph<int>();
        var vertex = _fixture.Create<int>();
        graph.AddVertex(vertex);

        var path = graph.GetShortestPath(vertex, vertex);

        path.ShouldContainSingle().Which.ShouldBe(vertex);
    }

    //#endregion

    //#region Connectivity Tests

    [Fact]
    public void IsConnected_ShouldReturnTrue_ForConnectedGraph()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "D");

        graph.IsConnected().ShouldBeTrue();
    }

    [Fact]
    public void IsConnected_ShouldReturnFalse_ForDisconnectedGraph()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddVertex("isolated");

        graph.IsConnected().ShouldBeFalse();
    }

    [Fact]
    public void IsConnected_ShouldThrow_ForDirectedGraph()
    {
        var graph = new SimpleGraph<string>(directed: true);

        var act = () => graph.IsConnected();

        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void IsStronglyConnected_ShouldWork_ForDirectedGraph()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        graph.IsStronglyConnected().ShouldBeTrue();
    }

    [Fact]
    public void IsStronglyConnected_ShouldReturnFalse_WhenNotStronglyConnected()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        graph.IsStronglyConnected().ShouldBeFalse();
    }

    //#endregion

    //#region Cycle Detection Tests

    [Fact]
    public void HasCycle_ShouldReturnTrue_WhenCycleExists_Undirected()
    {
        var graph = new SimpleGraph<string>(directed: false);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        graph.HasCycle().ShouldBeTrue();
    }

    [Fact]
    public void HasCycle_ShouldReturnFalse_WhenNoCycle_Undirected()
    {
        var graph = new SimpleGraph<string>(directed: false);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        graph.HasCycle().ShouldBeFalse();
    }

    [Fact]
    public void HasCycle_ShouldDetectCycles_Directed()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        graph.HasCycle().ShouldBeTrue();
    }

    [Fact]
    public void HasCycle_ShouldReturnFalse_ForTree_Directed()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("root", "child1");
        graph.AddEdge("root", "child2");

        graph.HasCycle().ShouldBeFalse();
    }

    //#endregion

    //#region Bipartite Tests

    [Fact]
    public void IsBipartite_ShouldReturnTrue_ForBipartiteGraph()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "1");
        graph.AddEdge("A", "2");
        graph.AddEdge("B", "1");
        graph.AddEdge("B", "2");

        graph.IsBipartite().ShouldBeTrue();
    }

    [Fact]
    public void IsBipartite_ShouldReturnFalse_ForOddCycle()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");
        graph.AddEdge("C", "A");

        graph.IsBipartite().ShouldBeFalse();
    }

    //#endregion

    //#region Connected Components Tests

    [Fact]
    public void GetConnectedComponents_ShouldReturnCorrectCount()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");
        graph.AddEdge("C", "D");
        graph.AddVertex("E");

        var components = graph.GetConnectedComponents().ToList();

        components.Should().HaveCount(3);
    }

    [Fact]
    public void GetConnectedComponents_ShouldThrow_ForDirected()
    {
        var graph = new SimpleGraph<string>(directed: true);

        var act = () => graph.GetConnectedComponents().ToList();

        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void GetStronglyConnectedComponents_ShouldWork_ForDirected()
    {
        var graph = new SimpleGraph<string>(directed: true);
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "A");
        graph.AddEdge("B", "C");

        var sccs = graph.GetStronglyConnectedComponents().ToList();

        sccs.Should().HaveCount(2);
    }

    //#endregion

    //#region Remove Tests

    [Fact]
    public void RemoveVertex_ShouldRemoveAndReturnTrue()
    {
        var graph = new SimpleGraph<string>();
        graph.AddVertex("remove");

        var result = graph.RemoveVertex("remove");

        result.ShouldBeTrue();
        graph.ContainsVertex("remove").ShouldBeFalse();
    }

    [Fact]
    public void RemoveVertex_ShouldRemoveIncidentEdges()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("remove", "keep");
        graph.AddEdge("keep", "other");

        graph.RemoveVertex("remove");

        graph.EdgeCount.ShouldBe(1);
    }

    [Fact]
    public void RemoveEdge_ShouldRemoveCorrectly()
    {
        var graph = new SimpleGraph<string>();
        graph.AddEdge("A", "B");

        graph.RemoveEdge("A", "B");

        graph.ContainsEdge("A", "B").ShouldBeFalse();
        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Clear Tests

    [Fact]
    public void Clear_ShouldResetGraph()
    {
        var graph = new SimpleGraph<int>();
        var vertices = _fixture.CreateMany<int>(10);
        foreach (var v in vertices)
            graph.AddVertex(v);

        graph.Clear();

        graph.VertexCount.ShouldBe(0);
        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Theory Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void VertexCount_ShouldBeAccurate(int count)
    {
        var graph = new SimpleGraph<int>();
        var vertices = _fixture.CreateMany<int>(count);

        foreach (var v in vertices)
            graph.AddVertex(v);

        graph.VertexCount.ShouldBe(count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsDirected_ShouldMatchConstructor(bool directed)
    {
        var graph = new SimpleGraph<string>(directed);

        graph.IsDirected.ShouldBe(directed);
    }

    //#endregion

    //#region Real-World Scenario Tests

    [Fact]
    public void SocialNetworkScenario_ShouldWorkCorrectly()
    {
        var graph = new SimpleGraph<string>(directed: false);
        var users = new[] { "Alice".Humanize(), "Bob".Humanize(), "Charlie".Humanize(), "Diana".Humanize() };
        
        foreach (var user in users)
            graph.AddVertex(user);
        
        graph.AddEdge("Alice", "Bob");
        graph.AddEdge("Alice", "Charlie");
        graph.AddEdge("Bob", "Charlie");
        graph.AddEdge("Charlie", "Diana");

        graph.VertexCount.ShouldBe(4);
        graph.EdgeCount.ShouldBe(4);
        graph.IsConnected().ShouldBeTrue();
        graph.IsBipartite().ShouldBeFalse();
        
        var path = graph.GetShortestPath("Alice", "Diana");
        path.ShouldNotBeEmpty();
        path.First().ShouldBe("Alice");
        path.Last().ShouldBe("Diana");
    }

    [Fact]
    public void TwitterFollowScenario_ShouldWorkCorrectly()
    {
        var graph = new SimpleGraph<string>(directed: true);
        
        graph.AddEdge("alice", "bob");
        graph.AddEdge("alice", "charlie");
        graph.AddEdge("bob", "alice");
        graph.AddEdge("charlie", "alice");

        graph.VertexCount.ShouldBe(3);
        graph.EdgeCount.ShouldBe(4);
        
        graph.GetNeighbors("alice").ShouldContain(new[] { "bob", "charlie" });
    }

    //#endregion
}
