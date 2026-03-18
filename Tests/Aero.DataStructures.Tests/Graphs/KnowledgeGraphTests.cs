using Shouldly;
using Aero.DataStructures.Graphs;
using Bogus;
using AutoFixture;
using Humanizer;

namespace Aero.DataStructures.Tests;

public class KnowledgeGraphTests
{
    private readonly Faker _faker = new();
    private readonly Fixture _fixture = new();

    //#region Ontology Tests

    [Fact]
    public void DefineClass_ShouldAddToOntology()
    {
        var kg = new KnowledgeGraph<string>();

        var entityClass = kg.DefineClass("Person", "A human being");

        entityClass.Name.ShouldBe("Person");
        entityClass.Description.ShouldBe("A human being");
        kg.ClassCount.ShouldBe(1);
    }

    [Fact]
    public void DefineClass_ShouldSupportInheritance()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Animal", "Living being");
        
        var dogClass = kg.DefineClass("Dog", "Canine animal", "Animal");

        dogClass.ParentClasses.ShouldContain("Animal");
    }

    [Fact]
    public void DefineRelation_ShouldAddToOntology()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.DefineClass("City", "Urban area");

        var relation = kg.DefineRelation("bornIn", "Person", "City", "Birthplace");

        relation.Name.ShouldBe("bornIn");
        relation.Domain.ShouldContain("Person");
        relation.Range.ShouldContain("City");
    }

    [Fact]
    public void DefineRelation_ShouldSupportProperties()
    {
        var kg = new KnowledgeGraph<string>();
        
        var relation = kg.DefineRelation("ancestorOf", "Person", "Person", 
            "Ancestry relation", inverseRelation: "descendantOf", 
            isTransitive: true, isSymmetric: false);

        relation.IsTransitive.ShouldBeTrue();
        relation.IsSymmetric.ShouldBeFalse();
        relation.InverseRelation.ShouldBe("descendantOf");
    }

    //#endregion

    //#region Entity Tests

    [Fact]
    public void AddEntity_ShouldCreateEntity()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human being");

        var entity = kg.AddEntity("einstein", "Person", new { name = "Albert Einstein" });

        entity.Id.ShouldBe("einstein");
        entity.Class.ShouldBe("Person");
        entity.Properties["name"].ShouldBe("Albert Einstein");
    }

    [Fact]
    public void AddEntity_ShouldSupportOptionalParameters()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");

        var entity = kg.AddEntity("user1", "Person", 
            label: "Test User".Humanize(), 
            confidence: 0.95, 
            source: "test");

        entity.Label.ShouldBe("Test user");
        entity.Confidence.ShouldBe(0.95);
        entity.Source.ShouldBe("test");
    }

    [Fact]
    public void GetEntity_ShouldReturnEntity()
    {
        var kg = new KnowledgeGraph<int>();
        var id = _fixture.Create<int>();
        kg.DefineClass("Item", "An item");
        kg.AddEntity(id, "Item");

        var entity = kg.GetEntity(id);

        entity.ShouldNotBeNull();
        entity!.Id.ShouldBe(id);
    }

    [Fact]
    public void GetEntitiesByClass_ShouldFilterCorrectly()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.DefineClass("City", "Urban area");
        kg.AddEntity("alice", "Person");
        kg.AddEntity("bob", "Person");
        kg.AddEntity("paris", "City");

        var persons = kg.GetEntitiesByClass("Person").ToList();

        persons.Count().ShouldBe(2);
        persons.All(e => e.Class == "Person").ShouldBeTrue();
    }

    //#endregion

    //#region Fact Tests

    [Fact]
    public void AddFact_ShouldCreateTriple()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.DefineClass("City", "Urban");
        kg.AddEntity("alice", "Person");
        kg.AddEntity("paris", "City");

        var fact = kg.AddFact("alice", "bornIn", "paris");

        fact.Subject.ShouldBe("alice");
        fact.Predicate.ShouldBe("bornIn");
        fact.Object.ShouldBe("paris");
        kg.TripleCount.ShouldBe(1);
    }

    [Fact]
    public void AddFact_ShouldThrow_WhenEntityDoesNotExist()
    {
        var kg = new KnowledgeGraph<string>();

        var act = () => kg.AddFact("unknown1", "relates", "unknown2");

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void AddFact_ShouldSupportSymmetricRelations()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.DefineRelation("friendOf", "Person", "Person", "Friends", 
            isSymmetric: true);
        kg.AddEntity("alice", "Person");
        kg.AddEntity("bob", "Person");

        kg.AddFact("alice", "friendOf", "bob");

        kg.TripleCount.ShouldBe(2);
        kg.HasFact("bob", "friendOf", "alice").ShouldBeTrue();
    }

    //#endregion

    //#region Query Tests

    [Fact]
    public void HasFact_ShouldReturnCorrectResult()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.AddEntity("a", "Person");
        kg.AddEntity("b", "Person");
        kg.AddFact("a", "knows", "b");

        kg.HasFact("a", "knows", "b").ShouldBeTrue();
        kg.HasFact("b", "knows", "a").ShouldBeFalse();
    }

    [Fact]
    public void GetFacts_WithPredicate_ShouldFilter()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.AddEntity("a", "Person");
        kg.AddEntity("b", "Person");
        kg.AddEntity("c", "Person");
        kg.AddFact("a", "knows", "b");
        kg.AddFact("a", "likes", "c");

        var knowsFacts = kg.GetFacts("knows").ToList();

        knowsFacts.ShouldHaveSingleItem();
        knowsFacts[0].Object.ShouldBe("b");
    }

    [Fact]
    public void GetFacts_WithSubjectAndPredicate_ShouldFilter()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.AddEntity("x", "Person");
        kg.AddEntity("y", "Person");
        kg.AddEntity("z", "Person");
        kg.AddFact("x", "knows", "y");
        kg.AddFact("z", "knows", "y");

        var xKnows = kg.GetFacts("x", "knows").ToList();

        xKnows.ShouldHaveSingleItem();
    }

    [Fact]
    public void GetConnectedEntities_ShouldReturnAllConnected()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Node", "A node");
        kg.AddEntity("center", "Node");
        kg.AddEntity("neighbor1", "Node");
        kg.AddEntity("neighbor2", "Node");
        kg.AddFact("center", "connects", "neighbor1");
        kg.AddFact("neighbor2", "connects", "center");

        var connected = kg.GetConnectedEntities("center").ToList();

        connected.Select(e => e.Id).ShouldBe(new[] { "neighbor1", "neighbor2" });
    }

    //#endregion

    //#region Inference Tests

    [Fact]
    public void InferTransitiveFacts_ShouldDeriveNewFacts()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.DefineRelation("ancestorOf", "Person", "Person", 
            "Ancestry", isTransitive: true);
        kg.AddEntity("grandparent", "Person");
        kg.AddEntity("parent", "Person");
        kg.AddEntity("child", "Person");
        kg.AddFact("grandparent", "ancestorOf", "parent");
        kg.AddFact("parent", "ancestorOf", "child");

        var inferred = kg.InferTransitiveFacts("ancestorOf").ToList();

        inferred.ShouldContain(f => 
            f.Subject == "grandparent" && f.Object == "child");
    }

    //#endregion

    //#region Find Entities Tests

    [Fact]
    public void FindEntities_ShouldFindByProperty()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Person", "Human");
        kg.AddEntity("alice", "Person", new { city = "NYC" });
        kg.AddEntity("bob", "Person", new { city = "LA" });
        kg.AddEntity("charlie", "Person", new { city = "NYC" });

        var nycResidents = kg.FindEntities("city", "NYC").ToList();

        nycResidents.Select(e => e.Id).ShouldBe(new[] { "alice", "charlie" });
    }

    //#endregion

    //#region Export Tests

    [Fact]
    public void ExportTriples_ShouldReturnAllFacts()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Node", "A node");
        kg.AddEntity("a", "Node");
        kg.AddEntity("b", "Node");
        kg.AddFact("a", "relates", "b");

        var triples = kg.ExportTriples().ToList();

        triples.ShouldHaveSingleItem();
        triples[0].Subject.ShouldBe("a");
        triples[0].Predicate.ShouldBe("relates");
        triples[0].Object.ShouldBe("b");
    }

    //#endregion

    //#region Clear Tests

    [Fact]
    public void Clear_ShouldRemoveAllData()
    {
        var kg = new KnowledgeGraph<string>();
        kg.DefineClass("Type", "A type");
        kg.DefineRelation("rel", "Type", "Type", "Relation");
        kg.AddEntity("e1", "Type");
        kg.AddFact("e1", "rel", "e1");

        kg.Clear();

        kg.EntityCount.ShouldBe(0);
        kg.TripleCount.ShouldBe(0);
        kg.ClassCount.ShouldBe(0);
        kg.RelationCount.ShouldBe(0);
    }

    //#endregion

    //#region Real-World Scenario Tests

    [Fact]
    public void ScientificKnowledgeGraph_ShouldModelCorrectly()
    {
        var kg = new KnowledgeGraph<string>();
        
        kg.DefineClass("Scientist", "A researcher");
        kg.DefineClass("Paper", "Academic publication");
        kg.DefineClass("Topic", "Research area");
        
        kg.DefineRelation("authored", "Scientist", "Paper", "Wrote");
        kg.DefineRelation("covers", "Paper", "Topic", "About");
        
        kg.AddEntity("einstein", "Scientist", new { name = "Albert Einstein" });
        kg.AddEntity("relativity", "Paper", new { title = "On Relativity" });
        kg.AddEntity("physics", "Topic", new { name = "Physics" });
        
        kg.AddFact("einstein", "authored", "relativity");
        kg.AddFact("relativity", "covers", "physics");

        kg.EntityCount.ShouldBe(3);
        kg.TripleCount.ShouldBe(2);
        
        var einsteinPapers = kg.GetFacts("einstein", "authored").ToList();
        einsteinPapers.ShouldHaveSingleItem();
    }

    //#endregion
}
