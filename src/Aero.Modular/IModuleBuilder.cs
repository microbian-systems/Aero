using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
/// Marker interface for field editors.
/// </summary>
public interface IFieldEditor { }

/// <summary>
/// Marker interface for search indexers.
/// </summary>
public interface ISearchIndexer { }

/// <summary>
/// Default implementation of IModuleBuilder that stores metadata contributions.
/// </summary>
public class AeroModuleBuilder : IAeroModuleBuilder
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _contentTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Type> _adminMenuContributors = new();
    private readonly List<Type> _shapeContributors = new();
    private readonly List<Type> _dashboardWidgets = new();
    private readonly List<Type> _contentParts = new();
    private readonly List<Type> _fieldEditors = new();
    private readonly List<Type> _searchIndexers = new();
    private readonly List<Type> _martenConfigurations = new();

    /// <summary>
    /// The service collection for DI registrations.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// The application configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// The host environment.
    /// </summary>
    public IHostEnvironment Environment { get; }

    /// <summary>
    /// Creates a new ModuleBuilder instance.
    /// </summary>
    public AeroModuleBuilder(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        Services = services;
        Configuration = configuration;
        Environment = environment;
    }

    /// <inheritdoc/>
    public IReadOnlySet<string> Permissions => _permissions;

    /// <inheritdoc/>
    public IReadOnlySet<string> ContentTypes => _contentTypes;

    /// <inheritdoc/>
    public IReadOnlyList<Type> AdminMenuContributors => _adminMenuContributors.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> ShapeContributors => _shapeContributors.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> DashboardWidgets => _dashboardWidgets.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> ContentParts => _contentParts.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> FieldEditors => _fieldEditors.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> SearchIndexers => _searchIndexers.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<Type> MartenConfigurations => _martenConfigurations.AsReadOnly();

    /// <inheritdoc/>
    public void AddPermission(string permission)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission);

        if (!_permissions.Add(permission))
        {
            throw new InvalidOperationException($"Permission '{permission}' has already been registered.");
        }
    }

    /// <inheritdoc/>
    public void AddAdminMenuContributor<T>() where T : class, IAdminMenuContributor
    {
        var type = typeof(T);
        if (_adminMenuContributors.Contains(type))
        {
            throw new InvalidOperationException($"Admin menu contributor '{type.FullName}' has already been registered.");
        }

        _adminMenuContributors.Add(type);
        Services.AddScoped<T>();
    }

    /// <inheritdoc/>
    public void AddShapeContributor<T>() where T : class, IShapeContributor
    {
        var type = typeof(T);
        if (_shapeContributors.Contains(type))
        {
            throw new InvalidOperationException($"Shape contributor '{type.FullName}' has already been registered.");
        }

        _shapeContributors.Add(type);
        Services.AddScoped<T>();
    }

    /// <inheritdoc/>
    public void AddDashboardWidget<T>() where T : class, IDashboardWidget
    {
        var type = typeof(T);
        if (_dashboardWidgets.Contains(type))
        {
            throw new InvalidOperationException($"Dashboard widget '{type.FullName}' has already been registered.");
        }

        _dashboardWidgets.Add(type);
        Services.AddScoped<T>();
    }

    /// <inheritdoc/>
    public void AddContentType(string contentType)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentType);

        if (!_contentTypes.Add(contentType))
        {
            throw new InvalidOperationException($"Content type '{contentType}' has already been registered.");
        }
    }

    /// <inheritdoc/>
    public void AddContentPart<TPart>() where TPart : class, IContentPart
    {
        var type = typeof(TPart);
        if (_contentParts.Contains(type))
        {
            throw new InvalidOperationException($"Content part '{type.FullName}' has already been registered.");
        }

        _contentParts.Add(type);
        Services.AddScoped<TPart>();
    }

    /// <inheritdoc/>
    public void AddFieldEditor<TEditor>() where TEditor : class, IFieldEditor
    {
        var type = typeof(TEditor);
        if (_fieldEditors.Contains(type))
        {
            throw new InvalidOperationException($"Field editor '{type.FullName}' has already been registered.");
        }

        _fieldEditors.Add(type);
        Services.AddScoped<TEditor>();
    }

    /// <inheritdoc/>
    public void AddSearchIndexer<TIndexer>() where TIndexer : class, ISearchIndexer
    {
        var type = typeof(TIndexer);
        if (_searchIndexers.Contains(type))
        {
            throw new InvalidOperationException($"Search indexer '{type.FullName}' has already been registered.");
        }

        _searchIndexers.Add(type);
        Services.AddScoped<TIndexer>();
    }

    /// <inheritdoc/>
    public void AddMartenConfiguration<T>() where T : class, global::Marten.IConfigureMarten
    {
        var type = typeof(T);
        if (_martenConfigurations.Contains(type))
        {
            throw new InvalidOperationException($"Marten configuration contributor '{type.FullName}' has already been registered.");
        }

        _martenConfigurations.Add(type);
        Services.AddSingleton<global::Marten.IConfigureMarten, T>();
    }
}
