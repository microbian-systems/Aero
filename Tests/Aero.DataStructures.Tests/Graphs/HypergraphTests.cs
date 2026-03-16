using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;
using Humanizer;

namespace Aero.DataStructures.Tests;

public class HypergraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Vertex Tests

    [Fact]
    public void AddVertex_ShouldIncreaseCount()
    {
        var graph = new Hypergraph<string, int>();
        var vertex = _faker.Name.FirstName();

        graph.AddVertex(vertex);

        graph.VertexCount.ShouldBe(1);
    }

    [Fact]
    public void AddVertex_ShouldReturnTrue_WhenNew()
    {
        var graph = new Hypergraph<int, int>();
        var vertex = _fixture.Create<int>();

        var result = graph.AddVertex(vertex);

        result.ShouldBeTrue();
    }

    [Fact]
    public void AddVertex_ShouldReturnFalse_WhenExists()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddVertex("existing");

        var result = graph.AddVertex("existing");

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddVertices_ShouldAddMultiple()
    {
        var graph = new Hypergraph<string, int>();
        var vertices = new[] { "A", "B", "C" };

        graph.AddVertices(vertices);

        graph.VertexCount.ShouldBe(3);
    }

    [Fact]
    public void Vertices_ShouldReturnAllVertices()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddVertex("a");
        graph.AddVertex("b");

        graph.Vertices.ShouldContain(new[] { "a", "b" });
    }

    //#endregion

    //#region Hyperedge Tests

    [Fact]
    public void AddHyperedge_ShouldConnectMultipleVertices()
    {
        var graph = new Hypergraph<string, int>();
        var vertices = new[] { "alice", "bob", "charlie" };

        var edge = graph.AddHyperedge(1, vertices, "Co-authorship".Humanize());

        edge.ShouldNotBeNull();
        edge!.Vertices.ShouldContain(vertices);
        edge.Cardinality.ShouldBe(3);
    }

    [Fact]
    public void AddHyperedge_ShouldAutoAddVertices()
    {
        var graph = new Hypergraph<string, int>();

        graph.AddHyperedge(1, new[] { "new1", "new2" });

        graph.VertexCount.ShouldBe(2);
    }

    [Fact]
    public void AddHyperedge_ShouldStoreData()
    {
        var graph = new Hypergraph<string, int>();
        var data = new { Title = "Paper Title", Year = 2023 };

        var edge = graph.AddHyperedge(1, new[] { "a", "b" }, data);

        edge!.Data.ShouldBe(data);
    }

    [Fact]
    public void AddHyperedge_ShouldReturnNull_WhenDuplicate()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });

        var result = graph.AddHyperedge(1, new[] { "c", "d" });

        result.ShouldBeNull();
    }

    [Fact]
    public void AddHyperedge_ShouldThrow_WhenEmpty()
    {
        var graph = new Hypergraph<string, int>();

        var act = () => graph.AddHyperedge(1, Array.Empty<string>());

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void AddHyperedge_ShouldSupportWeight()
    {
        var graph = new Hypergraph<string, int>();

        var edge = graph.AddHyperedge(1, new[] { "a", "b" }, weight: 2.5);

        edge!.Weight.ShouldBe(2.5);
    }

    //#endregion

    //#region Incidence Tests

    [Fact]
    public void GetIncidentHyperedges_ShouldReturnEdgesContainingVertex()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "alice", "bob" });
        graph.AddHyperedge(2, new[] { "alice", "charlie" });
        graph.AddHyperedge(3, new[] { "bob", "charlie" });

        var aliceEdges = graph.GetIncidentHyperedges("alice");

        aliceEdges.Select(e => e.Id).ShouldContain(new[] { 1, 2 });
        aliceEdges.Select(e => e.Id).ShouldNotContain(3);
    }

    [Fact]
    public void GetDegree_ShouldReturnIncidentEdgeCount()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "x", "a" });
        graph.AddHyperedge(2, new[] { "x", "b" });
        graph.AddHyperedge(3, new[] { "x", "c" });

        graph.GetDegree("x").ShouldBe(3);
    }

    //#endregion

    //#region Neighbor Tests

    [Fact]
    public void GetNeighbors_ShouldReturnVerticesInSameHyperedges()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "alice", "bob", "charlie" });
        graph.AddHyperedge(2, new[] { "alice", "diana" });

        var neighbors = graph.GetNeighbors("alice");

        neighbors.ShouldContain(new[] { "bob", "charlie", "diana" });
        neighbors.ShouldNotContain("alice");
    }

    //#endregion

    //#region Cardinality Tests

    [Fact]
    public void GetHyperedgesByCardinality_ShouldFilter()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });
        graph.AddHyperedge(2, new[] { "a", "b", "c" });
        graph.AddHyperedge(3, new[] { "x", "y" });

        var size2 = graph.GetHyperedgesByCardinality(2).ToList();

        size2.Select(e => e.Id).ShouldContain(new[] { 1, 3 });
        size2.Select(e => e.Id).ShouldNotContain(2);
    }

    [Fact]
    public void Rank_ShouldReturnMaxCardinality()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });
        graph.AddHyperedge(2, new[] { "a", "b", "c", "d" });
        graph.AddHyperedge(3, new[] { "x", "y", "z" });

        graph.Rank.ShouldBe(4);
    }

    //#endregion

    //#region Lookup Tests

    [Fact]
    public void HasHyperedge_ShouldCheckExactMatch()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b", "c" });

        graph.HasHyperedge(new[] { "a", "b", "c" }).ShouldBeTrue();
        graph.HasHyperedge(new[] { "a", "b" }).ShouldBeFalse();
    }

    [Fact]
    public void HasHyperedgeContaining_ShouldCheckSubset()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b", "c" });

        graph.HasHyperedgeContaining(new[] { "a", "b" }).ShouldBeTrue();
        graph.HasHyperedgeContaining(new[] { "a", "d" }).ShouldBeFalse();
    }

    //#endregion

    //#region Conversion Tests

    [Fact]
    public void ToCliqueGraph_ShouldConvertToRegularGraph()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b", "c" });

        var cliqueGraph = graph.ToCliqueGraph();

        cliqueGraph.ContainsEdge("a", "b").ShouldBeTrue();
        cliqueGraph.ContainsEdge("b", "c").ShouldBeTrue();
        cliqueGraph.ContainsEdge("a", "c").ShouldBeTrue();
    }

    [Fact]
    public void GetDual_ShouldSwapVerticesAndEdges()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });
        graph.AddHyperedge(2, new[] { "b", "c" });

        var dual = graph.GetDual();

        dual.VertexCount.ShouldBe(2);
        dual.HyperedgeCount.ShouldBe(3);
    }

    //#endregion

    //#region Traversal Tests

    [Fact]
    public void BreadthFirstTraversal_ShouldVisitConnectedVertices()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b", "c" });
        graph.AddHyperedge(2, new[] { "c", "d" });

        var result = graph.BreadthFirstTraversal("a").ToList();

        result.ShouldContain(new[] { "a", "b", "c", "d" });
    }

    //#endregion

    //#region Connected Components Tests

    [Fact]
    public void GetConnectedComponents_ShouldGroupCorrectly()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });
        graph.AddHyperedge(2, new[] { "c", "d" });

        var components = graph.GetConnectedComponents().ToList();

        components.Should().HaveCount(2);
    }

    //#endregion

    //#region Remove Tests

    [Fact]
    public void RemoveVertex_ShouldRemoveIncidentHyperedges()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "remove", "keep" });

        graph.RemoveVertex("remove");

        graph.HyperedgeCount.ShouldBe(0);
    }

    [Fact]
    public void RemoveHyperedge_ShouldNotRemoveVertices()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b" });

        graph.RemoveHyperedge(1);

        graph.VertexCount.ShouldBe(2);
        graph.HyperedgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Clear Tests

    [Fact]
    public void Clear_ShouldResetGraph()
    {
        var graph = new Hypergraph<string, int>();
        graph.AddHyperedge(1, new[] { "a", "b", "c" });
        graph.AddHyperedge(2, new[] { "x", "y" });

        graph.Clear();

        graph.VertexCount.ShouldBe(0);
        graph.HyperedgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Real-World Scenario Tests

    [Fact]
    public void CoAuthorshipScenario_ShouldWorkCorrectly()
    {
        var graph = new Hypergraph<string, int>();
        
        var paper1 = graph.AddHyperedge(1, new[] { "Alice", "Bob" }, 
            "Paper 1: Two authors".Humanize());
        var paper2 = graph.AddHyperedge(2, new[] { "Alice", "Bob", "Charlie" }, 
            "Paper 2: Three authors".Humanize());
        var paper3 = graph.AddHyperedge(3, new[] { "Charlie", "Diana" }, 
            "Paper 3: Two authors".Humanize());

        graph.VertexCount.ShouldBe(4);
        graph.HyperedgeCount.ShouldBe(3);
        
        var alicePapers = graph.GetIncidentHyperedges("Alice");
        alicePapers.Should().HaveCount(2);
        
        var aliceCoauthors = graph.GetNeighbors("Alice");
        aliceCoauthors.ShouldContain(new[] { "Bob", "Charlie" });
    }

    [Fact]
    public void GroupMembershipScenario_ShouldWorkCorrectly()
    {
        var graph = new Hypergraph<string, string>();
        
        graph.AddHyperedge("team-a", new[] { "emp1", "emp2", "emp3" });
        graph.AddHyperedge("team-b", new[] { "emp2", "emp4" });
        graph.AddHyperedge("committee", new[] { "emp1", "emp3", "emp5" });

        var emp1Groups = graph.GetIncidentHyperedges("emp1");
        emp1Groups.Select(e => e.Id).ShouldContain(new[] { "team-a", "committee" });
    }

    //#endregion
}
