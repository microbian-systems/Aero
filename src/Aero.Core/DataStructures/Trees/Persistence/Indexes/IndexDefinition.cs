using System.Linq.Expressions;

namespace Aero.Core.DataStructures.Trees.Persistence.Indexes;

/// <summary>
/// Represents a class for IndexDefinition.
/// </summary>
public class IndexDefinition
{
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public string Name { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public IndexType Type { get; init; }
        /// <summary>
    /// Gets or sets the Is Unique.
    /// </summary>
public bool IsUnique { get; init; }
        /// <summary>
    /// Gets or sets the Is Descending.
    /// </summary>
public bool IsDescending { get; init; }
        /// <summary>
    /// Gets or sets the Field Type.
    /// </summary>
public Type FieldType { get; init; } = typeof(object);
        /// <summary>
    /// Gets or sets the Field Name.
    /// </summary>
public string FieldName { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Root Page Id.
    /// </summary>
public long RootPageId { get; set; } = -1;
        /// <summary>
    /// Gets or sets the String Key Width.
    /// </summary>
public int StringKeyWidth { get; init; }
        /// <summary>
    /// Gets or sets the Is String Index.
    /// </summary>
public bool IsStringIndex => StringKeyWidth > 0;
}

/// <summary>
/// Represents a class for IndexDefinition.
/// </summary>
public sealed class IndexDefinition<TDocument, TField> : IndexDefinition
    where TField : unmanaged, IComparable<TField>
{
        /// <summary>
    /// Gets or sets the Key Extractor.
    /// </summary>
public required Func<TDocument, TField> KeyExtractor { get; init; }
        /// <summary>
    /// Gets or sets the Key Expression.
    /// </summary>
public required Expression<Func<TDocument, TField>> KeyExpression { get; init; }
}
