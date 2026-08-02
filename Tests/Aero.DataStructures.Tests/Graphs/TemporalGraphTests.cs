using TUnit.Core;
using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;

namespace Aero.DataStructures.Tests;

/// <summary>
/// Represents a class for TemporalGraphTests.
/// </summary>
public class TemporalGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Vertex Tests

        /// <summary>
    /// AddVertex_ShouldCreateTemporalVertex method.
    /// </summary>
[Test]
    public void AddVertex_ShouldCreateTemporalVertex()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = DateTime.Now;
        var end = start.AddDays(30);

        var vertex = graph.AddVertex("user1", start, end);

        vertex.Id.ShouldBe("user1");
        vertex.Lifetime.Start.ShouldBe(start);
        vertex.Lifetime.End.ShouldBe(end);
    }

        /// <summary>
    /// AddVertex_ShouldSupportOpenEndedLifetime method.
    /// </summary>
[Test]
    public void AddVertex_ShouldSupportOpenEndedLifetime()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = DateTime.Now;
        var farFuture = DateTime.MaxValue;

        var vertex = graph.AddVertex("active", start, farFuture);

        vertex.Lifetime.Contains(DateTime.Now.AddYears(10)).ShouldBeTrue();
    }

        /// <summary>
    /// VertexExistsAt_ShouldReturnCorrectResult method.
    /// </summary>
[Test]
    public void VertexExistsAt_ShouldReturnCorrectResult()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2023, 1, 1);
        graph.AddVertex("temporal", start, end);

        var vertex = graph.GetVertex("temporal");

        vertex!.ExistsAt(new DateTime(2021, 6, 1)).ShouldBeTrue();
        vertex.ExistsAt(new DateTime(2024, 1, 1)).ShouldBeFalse();
    }

    //#endregion

    //#region Edge Tests

        /// <summary>
    /// AddEdge_ShouldCreateTemporalEdge method.
    /// </summary>
[Test]
    public void AddEdge_ShouldCreateTemporalEdge()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = DateTime.Now;
        graph.AddVertex("a", start);
        graph.AddVertex("b", start);

        var edge = graph.AddEdge("a", "b", 1, start, start.AddDays(7));

        edge.ShouldNotBeNull();
        edge!.Source.ShouldBe("a");
        edge.Target.ShouldBe("b");
        edge.Lifetime.Start.ShouldBe(start);
    }

        /// <summary>
    /// AddEdge_ShouldThrow_WhenVerticesNotExist method.
    /// </summary>
[Test]
    public void AddEdge_ShouldThrow_WhenVerticesNotExist()
    {
        var graph = new TemporalGraph<string, int, DateTime>();

        var act = () => graph.AddEdge("nonexistent1", "nonexistent2", 1, DateTime.Now);

        act.ShouldThrow<ArgumentException>();
    }

        /// <summary>
    /// EdgeExistsAt_ShouldReturnCorrectResult method.
    /// </summary>
[Test]
    public void EdgeExistsAt_ShouldReturnCorrectResult()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2021, 1, 1);
        graph.AddVertex("a", start);
        graph.AddVertex("b", start);
        graph.AddEdge("a", "b", 1, start, end);

        var edge = graph.GetEdge(1);

        edge!.ExistsAt(new DateTime(2020, 6, 1)).ShouldBeTrue();
        edge.ExistsAt(new DateTime(2022, 1, 1)).ShouldBeFalse();
    }

    //#endregion

    //#region TimeInterval Tests

        /// <summary>
    /// TimeInterval_Contains_ShouldWorkCorrectly method.
    /// </summary>
[Test]
    public void TimeInterval_Contains_ShouldWorkCorrectly()
    {
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2021, 1, 1);
        var interval = new TemporalGraph<string, int, DateTime>.TimeInterval(start, end);

        interval.Contains(new DateTime(2020, 6, 1)).ShouldBeTrue();
        interval.Contains(new DateTime(2019, 12, 31)).ShouldBeFalse();
        interval.Contains(new DateTime(2021, 1, 1)).ShouldBeFalse();
    }

        /// <summary>
    /// TimeInterval_Overlaps_ShouldWorkCorrectly method.
    /// </summary>
[Test]
    public void TimeInterval_Overlaps_ShouldWorkCorrectly()
    {
        var interval1 = new TemporalGraph<string, int, DateTime>.TimeInterval(
            new DateTime(2020, 1, 1), new DateTime(2020, 6, 1));
        var interval2 = new TemporalGraph<string, int, DateTime>.TimeInterval(
            new DateTime(2020, 3, 1), new DateTime(2020, 9, 1));

        interval1.Overlaps(interval2).ShouldBeTrue();
    }

    //#endregion

    //#region Snapshot Tests

        /// <summary>
    /// GetSnapshot_ShouldReturnGraphAtTime method.
    /// </summary>
[Test]
    public void GetSnapshot_ShouldReturnGraphAtTime()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var t1 = new DateTime(2020, 1, 1);
        var t2 = new DateTime(2021, 1, 1);
        var tEnd = new DateTime(2025, 1, 1);
        
        graph.AddVertex("a", t1, tEnd);
        graph.AddVertex("b", t1, tEnd);
        graph.AddVertex("c", t2, tEnd);
        graph.AddEdge("a", "b", 1, t1, tEnd);

        var snapshot = graph.GetSnapshot(new DateTime(2020, 6, 1));

        snapshot.VertexCount.ShouldBe(2);
        snapshot.ContainsVertex("a").ShouldBeTrue();
        snapshot.ContainsVertex("c").ShouldBeFalse();
    }

        /// <summary>
    /// GetSnapshot_ShouldIncludeOnlyActiveEdges method.
    /// </summary>
[Test]
    public void GetSnapshot_ShouldIncludeOnlyActiveEdges()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = new DateTime(2020, 1, 1);
        var farFuture = new DateTime(2030, 1, 1);
        graph.AddVertex("a", start, farFuture);
        graph.AddVertex("b", start, farFuture);
        graph.AddVertex("c", start, farFuture);
        
        graph.AddEdge("a", "b", 1, start, new DateTime(2020, 6, 1));
        graph.AddEdge("a", "c", 2, new DateTime(2021, 1, 1), farFuture);

        var snapshot = graph.GetSnapshot(new DateTime(2020, 3, 1));

        snapshot.EdgeCount.ShouldBe(1);
        snapshot.ContainsEdge("a", "b").ShouldBeTrue();
        snapshot.ContainsEdge("a", "c").ShouldBeFalse();
    }

    //#endregion

    //#region Temporal Path Tests

        /// <summary>
    /// GetTemporalPaths_ShouldRespectTimeOrder method.
    /// </summary>
[Test]
    public void GetTemporalPaths_ShouldRespectTimeOrder()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var t1 = new DateTime(2020, 1, 1);
        var t2 = new DateTime(2020, 2, 1);
        var t3 = new DateTime(2020, 3, 1);
        
        graph.AddVertex("a", t1);
        graph.AddVertex("b", t1);
        graph.AddVertex("c", t1);
        
        graph.AddEdge("a", "b", 1, t2);
        graph.AddEdge("b", "c", 2, t3);

        var paths = graph.GetTemporalPaths("a", "c", t1).ToList();

        paths.ShouldNotBeEmpty();
        paths[0].Last().Vertex.ShouldBe("c");
    }

        /// <summary>
    /// GetEarliestArrival_ShouldComputeCorrectly method.
    /// </summary>
[Test]
    public void GetEarliestArrival_ShouldComputeCorrectly()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var t1 = new DateTime(2020, 1, 1);
        var t2 = new DateTime(2020, 2, 1);
        
        graph.AddVertex("start", t1);
        graph.AddVertex("end", t1);
        graph.AddEdge("start", "end", 1, t2);

        var arrival = graph.GetEarliestArrival("start", "end", t1);

        arrival.ShouldBe(t2);
    }

        /// <summary>
    /// GetEarliestArrival_ShouldReturnNull_WhenUnreachable method.
    /// </summary>
[Test]
    public void GetEarliestArrival_ShouldReturnNull_WhenUnreachable()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        graph.AddVertex("isolated", DateTime.Now, DateTime.MaxValue);
        graph.AddVertex("target", DateTime.Now, DateTime.MaxValue);

        var arrival = graph.GetEarliestArrival("isolated", "target", DateTime.Now);

        arrival.ShouldBeNull();
    }

    //#endregion

    //#region Edge Queries Tests

        /// <summary>
    /// GetEdgesInInterval_ShouldFilterCorrectly method.
    /// </summary>
[Test]
    public void GetEdgesInInterval_ShouldFilterCorrectly()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var baseTime = DateTime.Now;
        graph.AddVertex("a", baseTime);
        graph.AddVertex("b", baseTime);
        graph.AddVertex("c", baseTime);
        
        graph.AddEdge("a", "b", 1, baseTime, baseTime.AddDays(10));
        graph.AddEdge("b", "c", 2, baseTime.AddDays(5), baseTime.AddDays(15));
        graph.AddEdge("a", "c", 3, baseTime.AddDays(20), baseTime.AddDays(30));

        var edges = graph.GetEdgesInInterval(baseTime, baseTime.AddDays(7)).ToList();

        edges.Select(e => e.Id).ShouldBe(new[] { 1, 2 });
        edges.Select(e => e.Id).ShouldNotContain(3);
    }

        /// <summary>
    /// GetChangePoints_ShouldReturnAllChanges method.
    /// </summary>
[Test]
    public void GetChangePoints_ShouldReturnAllChanges()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var t1 = new DateTime(2020, 1, 1);
        var t2 = new DateTime(2020, 2, 1);
        var t3 = new DateTime(2020, 3, 1);
        
        graph.AddVertex("a", t1, t3);
        graph.AddVertex("b", t2);

        var changePoints = graph.GetChangePoints();

        changePoints.ShouldBe(new[] { t1, t2, t3 });
    }

    //#endregion

    //#region Remove Tests

        /// <summary>
    /// RemoveVertex_ShouldRemoveIncidentEdges method.
    /// </summary>
[Test]
    public void RemoveVertex_ShouldRemoveIncidentEdges()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = DateTime.Now;
        graph.AddVertex("remove", start);
        graph.AddVertex("keep", start);
        graph.AddEdge("remove", "keep", 1, start);

        graph.RemoveVertex("remove");

        graph.EdgeCount.ShouldBe(0);
    }

        /// <summary>
    /// RemoveEdge_ShouldKeepVertices method.
    /// </summary>
[Test]
    public void RemoveEdge_ShouldKeepVertices()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        var start = DateTime.Now;
        graph.AddVertex("a", start);
        graph.AddVertex("b", start);
        graph.AddEdge("a", "b", 1, start);

        graph.RemoveEdge(1);

        graph.VertexCount.ShouldBe(2);
        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Clear Tests

        /// <summary>
    /// Clear_ShouldResetGraph method.
    /// </summary>
[Test]
    public void Clear_ShouldResetGraph()
    {
        var graph = new TemporalGraph<string, int, DateTime>();
        graph.AddVertex("v1", DateTime.Now);
        graph.AddVertex("v2", DateTime.Now);
        graph.AddEdge("v1", "v2", 1, DateTime.Now);

        graph.Clear();

        graph.VertexCount.ShouldBe(0);
        graph.EdgeCount.ShouldBe(0);
    }

    //#endregion

    //#region Real-World Scenario Tests

        /// <summary>
    /// SocialNetworkEvolutionScenario_ShouldWorkCorrectly method.
    /// </summary>
[Test]
    public void SocialNetworkEvolutionScenario_ShouldWorkCorrectly()
    {
        var graph = new TemporalGraph<string, long, DateTime>();
        var registration = new DateTime(2020, 1, 1);
        var farFuture = new DateTime(2030, 1, 1);
        
        graph.AddVertex("alice", registration, farFuture);
        graph.AddVertex("bob", registration.AddDays(30), farFuture);
        graph.AddVertex("charlie", registration.AddDays(60), farFuture);
        
        var friendshipStart = new DateTime(2020, 6, 1);
        var friendshipEnd = new DateTime(2022, 6, 1);
        graph.AddEdge("alice", "bob", 1, friendshipStart, friendshipEnd);
        graph.AddEdge("bob", "charlie", 2, new DateTime(2020, 7, 1), farFuture);
        
        var snapshot2021 = graph.GetSnapshot(new DateTime(2021, 1, 1));
        var snapshot2023 = graph.GetSnapshot(new DateTime(2023, 1, 1));

        snapshot2021.EdgeCount.ShouldBe(2);
        snapshot2023.EdgeCount.ShouldBe(1);
        snapshot2023.ContainsEdge("bob", "charlie").ShouldBeTrue();
}

    //#endregion
}