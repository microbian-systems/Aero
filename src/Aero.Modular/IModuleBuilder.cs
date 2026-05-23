namespace Aero.Modular;

/// <summary>
/// A composition surface for modules to contribute metadata and services.
/// </summary>
public interface IAeroModuleBuilder
{
    /// <summary>
    /// Registers a permission that this module contributes.
    /// </summary>
    void AddPermission(string permission);

    /// <summary>
    /// Registers an admin menu contributor type.
    /// </summary>
    void AddAdminMenuContributor<T>() where T : class, IAdminMenuContributor;

    /// <summary>
    /// Registers a shape contributor type.
    /// </summary>
    void AddShapeContributor<T>() where T : class, IShapeContributor;

    /// <summary>
    /// Registers a dashboard widget type.
    /// </summary>
    void AddDashboardWidget<T>() where T : class, IDashboardWidget;

    /// <summary>
    /// Registers a content type that this module defines.
    /// </summary>
    void AddContentType(string contentType);

    /// <summary>
    /// Registers a content part type.
    /// </summary>
    void AddContentPart<TPart>() where TPart : class, IContentPart;

    /// <summary>
    /// Registers a field editor type.
    /// </summary>
    void AddFieldEditor<TEditor>() where TEditor : class, IFieldEditor;

    /// <summary>
    /// Registers a search indexer type.
    /// </summary>
    void AddSearchIndexer<TIndexer>() where TIndexer : class, ISearchIndexer;

    /// <summary>
    /// Gets the registered permissions.
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    /// Gets the registered content types.
    /// </summary>
    IReadOnlySet<string> ContentTypes { get; }

    /// <summary>
    /// Gets the registered admin menu contributor types.
    /// </summary>
    IReadOnlyList<Type> AdminMenuContributors { get; }

    /// <summary>
    /// Gets the registered shape contributor types.
    /// </summary>
    IReadOnlyList<Type> ShapeContributors { get; }

    /// <summary>
    /// Gets the registered dashboard widget types.
    /// </summary>
    IReadOnlyList<Type> DashboardWidgets { get; }

    /// <summary>
    /// Gets the registered content part types.
    /// </summary>
    IReadOnlyList<Type> ContentParts { get; }

    /// <summary>
    /// Gets the registered field editor types.
    /// </summary>
    IReadOnlyList<Type> FieldEditors { get; }

    /// <summary>
    /// Gets the registered search indexer types.
    /// </summary>
    IReadOnlyList<Type> SearchIndexers { get; }

    /// <summary>
    /// Gets the registered Marten configuration contributor types.
    /// </summary>
    IReadOnlyList<Type> MartenConfigurations { get; }

    /// <summary>
    /// Registers a Marten schema configuration contributor type.
    /// The type must implement <see cref="global::Marten.IConfigureMarten"/>.
    /// </summary>
    void AddMartenConfiguration<T>() where T : class, global::Marten.IConfigureMarten;
}

/// <summary>
/// Marker interface for admin menu contributors.
/// </summary>
public interface IAdminMenuContributor { }

/// <summary>
/// Marker interface for shape contributors.
/// </summary>
public interface IShapeContributor { }

/// <summary>
/// Marker interface for dashboard widgets.
/// </summary>
public interface IDashboardWidget { }

/// <summary>
/// Marker interface for content parts.
/// </summary>
public interface IContentPart { }

/// <summary>
/// Defines a field editor for content types in the admin UI.
/// Implementations provide the metadata needed to render and normalize
/// editor values for a specific field type (e.g. "text", "image", "reference").
/// </summary>
public interface IFieldEditor
{
    /// <summary>The field type alias this editor handles (e.g. "text", "image", "reference").</summary>
    string FieldType { get; }

    /// <summary>The Blazor component name used in the admin UI (e.g. "aero-textbox").</summary>
    string EditorComponent { get; }

    /// <summary>Normalizes a raw editor value before storage.</summary>
    object? Normalize(object? value);
}

/// <summary>
/// Marker interface for search indexers.
/// </summary>
public interface ISearchIndexer { }


