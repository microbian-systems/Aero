using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.DataStructures.Graphs;

/// <summary>
/// Defines an interface for IGraphServiceAsync.
/// </summary>
public interface IGraphServiceAsync<TEntity, TKey> 
    where TEntity : class 
    where TKey : IEquatable<TKey> //IEquatableEntryGraphIterator<TKey>
{

}

/// <summary>
/// Defines an interface for IGraphService.
/// </summary>
public interface IGraphService<TEntity, TKey>  
    where TEntity : class  , IEntity<TKey>
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Get method.
    /// </summary>
IEnumerable<TEntity> Get(int limit, int skip);
        /// <summary>
    /// Get method.
    /// </summary>
IEnumerable<TEntity> Get(string label);
        /// <summary>
    /// Get method.
    /// </summary>
IEnumerable<TEntity> Get(string label, int limit, int skip);
        /// <summary>
    /// GetLabels method.
    /// </summary>
IEnumerable<string> GetLabels(TKey id);
        /// <summary>
    /// AddLabel method.
    /// </summary>
bool AddLabel(TKey id, string label);
        /// <summary>
    /// DeleteLabel method.
    /// </summary>
bool DeleteLabel(TKey id, string label);
        /// <summary>
    /// GetRelated method.
    /// </summary>
IEnumerable<TOut> GetRelated<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>;
        /// <summary>
    /// GetRelated method.
    /// </summary>
IEnumerable<TOut> GetRelated<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class;
        /// <summary>
    /// GetRelatedCount method.
    /// </summary>
int GetRelatedCount<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>;
        /// <summary>
    /// GetRelatedCount method.
    /// </summary>
int GetRelatedCount<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class;
        /// <summary>
    /// AddRelationShip method.
    /// </summary>
bool AddRelationShip<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>;
        /// <summary>
    /// AddRelationShip method.
    /// </summary>
bool AddRelationShip<TOut, TRelation>(TKey inboundId, TKey outboundId, string relationship, TRelation relation) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class;
        /// <summary>
    /// HasRelationship method.
    /// </summary>
bool HasRelationship<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>;
        /// <summary>
    /// DeleteRelationShip method.
    /// </summary>
bool DeleteRelationShip<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>;
        /// <summary>
    /// CreateConstraint method.
    /// </summary>
bool CreateConstraint();
        /// <summary>
    /// CreateIndex method.
    /// </summary>
bool CreateIndex();
        /// <summary>
    /// CreateIndex method.
    /// </summary>
bool CreateIndex(string property);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string label, Expression<Func<TEntity, bool>> predicate);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string label, string expression);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string expression);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string label, Expression<Func<TEntity, bool>> predicate, int limit, int skip);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string label, string expression, int limit, int skip);
        /// <summary>
    /// Find method.
    /// </summary>
IQueryable<TEntity> Find(string expression, int limit, int skip);
}