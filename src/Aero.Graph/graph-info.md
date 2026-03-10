# RavenDB Graph Database Information

## Overview
RavenDB includes a graph database feature for querying document relationships using a custom graph query language. The graph API is marked as **Obsolete** in the current version.

---

## Core Graph Query Implementation (Server)

### Main Graph Directory
`src/Raven.Server/Documents/Queries/Graph/`

| File | Purpose |
|------|---------|
| `GraphQueryRunner.cs` | Main entry point for graph query execution |
| `GraphQueryRunner.Match.cs` | Match clause processing |
| `GraphQueryPlan.cs` | Graph query plan representation |
| `IGraphQueryStep.cs` | Interface for query steps |
| `EdgeQueryStep.cs` | Edge traversal logic |
| `RecursionQueryStep.cs` | Recursive graph traversal |
| `QueryQueryStep.cs` | Query step implementation |
| `QueryQueryStepGatherer.cs` | Gathers query steps |
| `CollectionDestinationQueryStep.cs` | Collection destination handling |
| `IntersectionQueryStep.cs` | Intersection operations |
| `QueryPlanVisitor.cs` | Visitor pattern for query plans |
| `QueryPlanRewriter.cs` | Rewrites graph query plans |
| `GraphQueryIndexGatherer.cs` | Index gathering for graphs |
| `GraphQueryDetailedReporter.cs` | Query reporting |
| `Union.cs`, `Intersection.cs`, `Except.cs` | Set operations |
| `SingleEdgeMatcher.cs` | Edge matching logic |
| `EdgeCollectionDestinationRewriter.cs` | Edge destination rewriting |

---

## AST/Parser (Server)

| File | Purpose |
|------|---------|
| `src/Raven.Server/Documents/Queries/AST/GraphQuery.cs` | Graph query AST nodes |
| `src/Raven.Server/Documents/Queries/AST/GraphQuerySyntaxValidatorVisitor.cs` | Syntax validation |
| `src/Raven.Server/Documents/Queries/AST/WithEdgesExpression.cs` | `withEdges()` expression |

---

## Query Execution (Server)

| File | Purpose |
|------|---------|
| `src/Raven.Server/Documents/Queries/Results/GraphQueryResultRetriever.cs` | Results retrieval |
| `src/Raven.Server/Documents/Queries/QueryRunner.cs` | Query runner (includes graph) |
| `src/Raven.Server/Documents/Queries/QueryMetadata.cs` | Query metadata |
| `src/Raven.Server/Documents/Handlers/QueriesHandler.cs` | HTTP handler |

---

## Client-Side (Raven.Client)

| File | Purpose |
|------|---------|
| `src/Raven.Client/Documents/Session/DocumentSession.GraphQuery.cs` | Graph query session methods |
| `src/Raven.Client/Documents/Session/Tokens/GraphQueryToken.cs` | Query token serialization |
| `src/Raven.Client/Documents/Session/Tokens/WithEdgesToken.cs` | WithEdges token |
| `src/Raven.Client/Documents/Session/DocumentQuery.cs` | Document query (graph support) |
| `src/Raven.Client/Documents/Session/AsyncDocumentQuery.cs` | Async document query |

### Client Interface
- `IGraphQuery<T>` - Marked as obsolete

---

## LINQ Provider (Client)

### Location
`src/Raven.Client/Documents/Linq/`

| File | Purpose |
|------|---------|
| `RavenQueryProvider.cs` | Main LINQ query provider |
| `RavenQueryProviderProcessor.cs` | Expression processing |
| `RavenQueryableExtensions.cs` | LINQ extensions |
| `QueryMethodConverter.cs` | Method conversions |

### LINQ for Graph
**NO LINQ provider exists for graph queries.** Graph queries only work via raw string queries.

---

## Tests

### FastTests
- `test/FastTests/Graph/BasicGraphQueries.cs`
- `test/FastTests/Graph/ClientGraphQueries.cs`
- `test/FastTests/Graph/Parsing.cs`

### SlowTests
- `test/SlowTests/Graph/BasicGraphQueries.cs`
- `test/SlowTests/Graph/AdvancedGraphQueries.cs`
- `test/SlowTests/Graph/IntersectionTests.cs`
- `test/SlowTests/Graph/GraphPermissionTests.cs`
- `test/SlowTests/Graph/SortGraphQueries.cs`
- `test/SlowTests/Graph/VerticesFromIndexes.cs`
- `test/SlowTests/Graph/RavenDB-15453.cs`

---

## Studio (Frontend)

| File | Purpose |
|------|---------|
| `src/Raven.Studio/typescript/common/query/graphQueryResults.ts` | Graph query results UI |
| `src/Raven.Studio/typescript/commands/database/query/debugGraphOutputCommand.ts` | Debug command |
| `src/Raven.Studio/wwwroot/Content/css/pages/database-group-graph.less` | Graph visualization CSS |

---

## Graph Query Syntax (Custom - NOT Cypher)

RavenDB uses its own proprietary graph query language, NOT Cypher.

### Key Clauses

| Clause | Description |
|--------|-------------|
| `with { from Collection } as alias` | Define a vertex source |
| `match (alias)-[Edge]->(alias2)` | Define graph traversal pattern |
| `select` | Project results |
| `-[...]->` | Edge traversal with optional filters |
| `recursive { [Edge]->(Vertex) }` | Recursive traversal |
| `where` | Filter vertices or edges |
| `and` / `or` / `not` | Boolean operators |
| `Limit n,m` | Pagination |

### Syntax Examples

```csharp
// Simple match
"match (Dogs as a)-[Likes]->(Dogs as f)<-[Likes]-(Dogs as b)"

// With vertex filtering
"match (Users as u where id() = 'users/1')-[Rated as r where Rating > 4]->(Movies as m)"

// Edge projection
"match (Orders as o where id() = 'orders/821-A')-[Lines select Product]->(Products as p)"

// Recursive
"match (Employees as e)-recursive { [ReportsTo]->(Employees as boss) }"

// Multiple patterns
"match (Users as u)<-[Rated as r]-(Movies as m) and (Actors as actor)-[ActedOn]->(m)"

// With JavaScript functions
@"declare function includeProducts(doc) { ... }
 match (Orders as o) select includeProducts(o)"

// Include documents
"match (Orders where id() = 'orders/821-A') include Lines.Product"
```

### Differences from Cypher

| Feature | RavenDB | Cypher |
|---------|---------|--------|
| Projection | `select` | `RETURN` |
| Vertex source | `with { from Collection }` | `MATCH (n:Label)` |
| Edge traversal | `-[Edge select Field]->` | `-[r:RELATIONSHIP]->` |
| Recursion | `-recursive { ... }` | `MATCH (a)-[:REL*1..]->(b)` |

---

## Key Findings

### 1. Graph API is Obsolete
The `IGraphQuery<T>` interface is marked with `[Obsolete(Constants.Obsolete.GraphApi)]`.

### 2. No LINQ Provider
RavenDB does NOT have a LINQ provider for graph queries. Only raw string-based queries are supported via `session.Advanced.RawQuery<T>(queryString)`.

### 3. Custom Syntax
The graph query language is proprietary to RavenDB and is NOT compatible with Cypher (Neo4j) or other graph query languages.

### 4. Implementation Scope
- Core implementation: ~18 files in `src/Raven.Server/Documents/Queries/Graph/`
- Tests: ~10 test files covering parsing, basic queries, advanced queries, intersections, permissions, sorting
- Client API: Minimal - only raw string query support

---

## Related Patterns Found

### Grep Results Summary
- `Graph`: 135 files
- `IGraphQueryStep`: 19 files
- `GraphQuery`: 51 files
- `WithEdges`: 16 files

---

## Notes from Discussion

1. User asked about LINQ provider for graph queries - Answer: No LINQ provider exists
2. User asked if syntax is Cypher - Answer: No, it's a custom proprietary syntax
3. Graph API is marked obsolete - may be deprecated in future versions
4. Query execution uses Lucene search engine under the hood
5. Graph queries can work with indexes (VerticesFromIndexes tests)

---

*Last Updated: 2026-03-03*
