using System.Linq.Expressions;
using Aero.Core.DataStructures.Trees.Persistence.Indexes;

namespace Aero.Core.DataStructures.Trees.Persistence.Linq.Translation;

/// <summary>
/// Represents a record for FilterClause.
/// </summary>
public abstract record FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public abstract bool IsIndexSatisfiable { get; }
}

/// <summary>
/// Represents a record for ComparisonFilter.
/// </summary>
public sealed record ComparisonFilter(
    MemberAccess Member,
    ExpressionType Operator,
    object Value
) : FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public override bool IsIndexSatisfiable =>
        Member.IndexDefinition is not null && IsSupportedOperator;

    private bool IsSupportedOperator => Operator is
        ExpressionType.Equal or
        ExpressionType.NotEqual or
        ExpressionType.GreaterThan or
        ExpressionType.GreaterThanOrEqual or
        ExpressionType.LessThan or
        ExpressionType.LessThanOrEqual;
}

/// <summary>
/// Represents a record for AndFilter.
/// </summary>
public sealed record AndFilter(FilterClause Left, FilterClause Right) : FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public override bool IsIndexSatisfiable => Left.IsIndexSatisfiable || Right.IsIndexSatisfiable;
}

/// <summary>
/// Represents a record for OrFilter.
/// </summary>
public sealed record OrFilter(FilterClause Left, FilterClause Right) : FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public override bool IsIndexSatisfiable => false;
}

/// <summary>
/// Represents a record for NotFilter.
/// </summary>
public sealed record NotFilter(FilterClause Inner) : FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public override bool IsIndexSatisfiable => false;
}

/// <summary>
/// Represents a record for MethodFilter.
/// </summary>
public sealed record MethodFilter(
    MemberAccess Target,
    string MethodName,
    IReadOnlyList<object> Arguments
) : FilterClause
{
        /// <summary>
    /// Gets or sets the Is Index Satisfiable.
    /// </summary>
public override bool IsIndexSatisfiable =>
        MethodName is "StartsWith" && Target.IndexDefinition is not null;
}

/// <summary>
/// Represents a record for MemberAccess.
/// </summary>
public sealed record MemberAccess(
    string FieldName,
    IndexDefinition? IndexDefinition
);

/// <summary>
/// Represents a record for OrderClause.
/// </summary>
public sealed record OrderClause(
    MemberAccess Member,
    bool Descending
);
