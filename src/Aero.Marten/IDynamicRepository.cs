namespace Aero.Marten;

// todo - finalize the dynamic marten repository - the TKey parameter shouldn't need to be declared at the class level
/// <summary>
/// Defines an interface for IDynamicReadOnlyRepositoryAsync.
/// </summary>
public interface IDynamicReadOnlyRepositoryAsync<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// InvalidateCacheAsync method.
    /// </summary>
Task InvalidateCacheAsync<T>(IEnumerable<T> documents) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// CountAsync method.
    /// </summary>
Task<long> CountAsync<T>() where T : class, IEntity<TKey>, new();
        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
Task<T> GetByIdAsync<T>(TKey id) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// GetByIdsAsync method.
    /// </summary>
Task<IReadOnlyCollection<T>> GetByIdsAsync<T>(List<TKey> ids) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
Task<IEnumerable<T>> GetAllAsync<T>() where T : class, IEntity<TKey>, new();
        /// <summary>
    /// ExistsAsync method.
    /// </summary>
Task<bool> ExistsAsync<T>(TKey id) where T : class, IEntity<TKey>, new();

        /// <summary>
    /// Search method.
    /// </summary>
Task<IEnumerable<T>> Search<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity<TKey>, new();

        /// <summary>
    /// FindSingle method.
    /// </summary>
Task<T> FindSingle<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity<TKey>, new();
}

/// <summary>
/// Defines an interface for IDynamicRepositoryAsync.
/// </summary>
public interface IDynamicRepositoryAsync<TKey> : IDynamicReadOnlyRepositoryAsync<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// SaveAsync method.
    /// </summary>
Task<T> SaveAsync<T>(T document) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// SaveAsync method.
    /// </summary>
Task SaveAsync<T>(IEnumerable<T> documents) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync<T>(TKey id) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync<T>(List<TKey> ids) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync<T>(T document) where T : class, IEntity<TKey>, new();
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync<T>(IEnumerable<T> documents) where T : class, IEntity<TKey>, new();
    //Task<long> DeleteAllAsync<T>();

    //#region "unused from Foundatio.Repositories
    // todo - impl AsyncEvents like Foundatio.Repositories e.g. below
    //        AsyncEvent<BeforeQueryEventArgs<T>> BeforeQuery { get; set; }
    //        AsyncEvent<DocumentsEventArgs<T>> DocumentsAdding { get; set; }
    //        AsyncEvent<DocumentsEventArgs<T>> DocumentsAdded { get; set; }
    //        AsyncEvent<ModifiedDocumentsEventArgs<T>> DocumentsSaving { get; set; }
    //        AsyncEvent<ModifiedDocumentsEventArgs<T>> DocumentsSaved { get; set; }
    //        AsyncEvent<DocumentsEventArgs<T>> DocumentsRemoving { get; set; }
    //        AsyncEvent<DocumentsEventArgs<T>> DocumentsRemoved { get; set; }
    //        AsyncEvent<DocumentsChangeEventArgs<T>> DocumentsChanging { get; set; }
    //        AsyncEvent<DocumentsChangeEventArgs<T>> DocumentsChanged { get; set; }
    //#endregion
}