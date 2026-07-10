using Aero.Core.DataStructures.Trees.Persistence.Indexes;

namespace Aero.Core.DataStructures.Trees.Persistence.Linq.Translation;

/// <summary>
/// Represents a class for TranslatedQuery.
/// </summary>
public sealed class TranslatedQuery<TDocument>
{
        /// <summary>
    /// Gets or sets the Filters.
    /// </summary>
public IReadOnlyList<FilterClause> Filters { get; init; } = Array.Empty<FilterClause>();
        /// <summary>
    /// Gets or sets the Order Bys.
    /// </summary>
public IReadOnlyList<OrderClause> OrderBys { get; init; } = Array.Empty<OrderClause>();
        /// <summary>
    /// Gets or sets the Take.
    /// </summary>
public int? Take { get; init; }
        /// <summary>
    /// Gets or sets the Skip.
    /// </summary>
public int? Skip { get; init; }
        /// <summary>
    /// Gets or sets the Residual Predicate.
    /// </summary>
public Func<TDocument, bool>? ResidualPredicate { get; set; }
        /// <summary>
    /// Gets or sets the Selected Index Scan.
    /// </summary>
public IndexScanSpec? SelectedIndexScan { get; set; }
}

/// <summary>
/// Represents a class for IndexScanSpec.
/// </summary>
public sealed class IndexScanSpec
{
        /// <summary>
    /// Gets or sets the Index.
    /// </summary>
public required IndexDefinition Index { get; init; }
        /// <summary>
    /// Gets or sets the From.
    /// </summary>
public object? From { get; init; }
        /// <summary>
    /// Gets or sets the To.
    /// </summary>
public object? To { get; init; }
        /// <summary>
    /// Gets or sets the Is Point.
    /// </summary>
public bool IsPoint { get; init; }
        /// <summary>
    /// Gets or sets the Include From.
    /// </summary>
public bool IncludeFrom { get; init; } = true;
        /// <summary>
    /// Gets or sets the Include To.
    /// </summary>
public bool IncludeTo { get; init; } = true;
}
