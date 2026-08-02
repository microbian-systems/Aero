using System.Linq.Expressions;
using Aero.Core.Data;
using Aero.Core.DataStructures.Graphs;
using Aero.Core.Entities;

namespace Aero.Services;

/// <summary>
/// Represents a class for GraphService.
/// </summary>
public class GraphService<TEntity, TKey>(IGraphRepository<TEntity, TKey> repository) : IGraphService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
        /// <summary>
    /// _repository.
    /// </summary>
protected readonly IGraphRepository<TEntity, TKey> _repository = repository;

        /// <summary>
    /// Add method.
    /// </summary>
public TEntity Add(TEntity entity)
    {
        // todo - fix add in grapchservice
        //return _repository.ad(entity);

        throw new NotImplementedException();
    }

        /// <summary>
    /// AddAsync method.
    /// </summary>
public Task<TEntity> AddAsync(TEntity entity)
    {
        return Task.FromResult(Add(entity));
    }

        /// <summary>
    /// AddLabel method.
    /// </summary>
public bool AddLabel(TKey id, string label)
    {
        return _repository.AddLabel(id, label);
    }

        /// <summary>
    /// AddLabelAsync method.
    /// </summary>
public Task<bool> AddLabelAsync(TKey id, string label)
    {
        return Task.FromResult(AddLabel(id, label));
    }

        /// <summary>
    /// AddRelationShip method.
    /// </summary>
public bool AddRelationShip<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return _repository.AddRelationShip<TOut>(inboundId, outboundId, relationship);
    }

        /// <summary>
    /// AddRelationShipAsync method.
    /// </summary>
public Task<bool> AddRelationShipAsync<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(AddRelationShip<TOut>(inboundId, outboundId, relationship));
    }

        /// <summary>
    /// CreateConstraint method.
    /// </summary>
public bool CreateConstraint()
    {
        return _repository.CreateConstraint();
    }

        /// <summary>
    /// CreateConstraintAsync method.
    /// </summary>
public Task<bool> CreateConstraintAsync()
    {
        return Task.FromResult(CreateConstraint());
    }

        /// <summary>
    /// CreateIndex method.
    /// </summary>
public bool CreateIndex()
    {
        return _repository.CreateIndex();
    }

        /// <summary>
    /// CreateIndex method.
    /// </summary>
public bool CreateIndex(string property)
    {
        return _repository.CreateIndex(property);
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string label, Expression<Func<TEntity, bool>> predicate)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string label, string expression)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string expression)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// CreateIndexAsync method.
    /// </summary>
public Task<bool> CreateIndexAsync()
    {
        return Task.FromResult(CreateIndex());
    }

        /// <summary>
    /// CreateIndexAsync method.
    /// </summary>
public Task<bool> CreateIndexAsync(string property)
    {
        return Task.FromResult(CreateIndex(property));
    }

        /// <summary>
    /// Delete method.
    /// </summary>
public void Delete(TEntity entity)
    {
        // todo - fix delete method in graph service    
        throw new NotImplementedException();
        //_repository.Delete(entity);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public Task DeleteAsync(TEntity entity)
    {
        return Task.Run(() => Delete(entity));
    }

        /// <summary>
    /// DeleteLabel method.
    /// </summary>
public bool DeleteLabel(TKey id, string label)
    {
        return _repository.DeleteLabel(id, label);
    }

        /// <summary>
    /// DeleteLabelAsync method.
    /// </summary>
public Task<bool> DeleteLabelAsync(TKey id, string label)
    {
        return Task.FromResult(DeleteLabel(id, label));
    }

        /// <summary>
    /// DeleteRelationShip method.
    /// </summary>
public bool DeleteRelationShip<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return _repository.DeleteRelationShip<TOut>(inboundId, outboundId, relationship);
    }

        /// <summary>
    /// DeleteRelationShipAsync method.
    /// </summary>
public Task<bool> DeleteRelationShipAsync<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(DeleteRelationShip<TOut>(inboundId, outboundId, relationship));
    }

    //public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    //{
    //    return _repository.Find(predicate);
    //}

    //public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    //{
    //    return Task.FromResult(Find(predicate));
    //}

    //public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> expression)
    //{
    //    return _repository.Find(expression);
    //}

    //public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> label, Expression<Func<TEntity, bool>> predicate)
    //{
    //    return _repository.Find(label, predicate);
    //}

    //public IQueryable<TEntity> Find(string label, Expression<Func<TEntity, bool>> expression)
    //{
    //    return _repository.Find(label, expression);
    //}

    //public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression)
    //{
    //    return Task.FromResult(Find(expression));
    //}

    //public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> label, Expression<Func<TEntity, bool>> predicate)
    //{
    //    return Task.FromResult(Find(label, predicate));
    //}

    //public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> label, string expression)
    //{
    //    return Task.FromResult(Find(label, expression));
    //}

    // todo - implement get methods in graph service
        /// <summary>
    /// Get method.
    /// </summary>
public IEnumerable<TEntity> Get()
    {
        throw new NotImplementedException();
        //return _repository.Get();
    }

        /// <summary>
    /// Get method.
    /// </summary>
public IEnumerable<TEntity> Get(int limit, int skip)
    {
        throw new NotImplementedException();
        //return _repository.Get();
    }

        /// <summary>
    /// GetAsync method.
    /// </summary>
public Task<IEnumerable<TEntity>> GetAsync()
    {
        return Task.FromResult(Get());
    }

        /// <summary>
    /// GetAsync method.
    /// </summary>
public Task<IEnumerable<TEntity>> GetAsync(int limit, int skip)
    {
        return Task.FromResult(Get(limit, skip));
    }

        /// <summary>
    /// GetById method.
    /// </summary>
public TEntity GetById(TKey id)
    {
        throw new NotImplementedException();
        //return _repository.GetById(id);
    }

        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
public Task<TEntity> GetByIdAsync(TKey id)
    {
        return Task.FromResult(GetById(id));
    }

        /// <summary>
    /// Get method.
    /// </summary>
public IEnumerable<TEntity> Get(string label)
    {
        return _repository.Get(label);
    }

        /// <summary>
    /// GetAsync method.
    /// </summary>
public Task<IEnumerable<TEntity>> GetAsync(string label)
    {
        return Task.FromResult(Get(label));
    }

        /// <summary>
    /// GetLabels method.
    /// </summary>
public IEnumerable<string> GetLabels(TKey id)
    {
        return _repository.GetLabels(id);
    }

        /// <summary>
    /// GetLabelsAsync method.
    /// </summary>
public Task<IEnumerable<string>> GetLabelsAsync(TKey id)
    {
        return Task.FromResult(GetLabels(id));
    }

        /// <summary>
    /// GetRelated method.
    /// </summary>
public IEnumerable<TOut> GetRelated<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return _repository.GetRelated<TOut>(id, relationship);
    }

        /// <summary>
    /// GetRelatedAsync method.
    /// </summary>
public Task<IEnumerable<TOut>> GetRelatedAsync<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(GetRelated<TOut>(id, relationship));
    }

        /// <summary>
    /// GetRelatedCount method.
    /// </summary>
public int GetRelatedCount<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return _repository.GetRelatedCount<TOut>(id, relationship);
    }

        /// <summary>
    /// GetRelatedCountAsync method.
    /// </summary>
public Task<int> GetRelatedCountAsync<TOut>(TKey id, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(GetRelatedCount<TOut>(id, relationship));
    }

        /// <summary>
    /// Update method.
    /// </summary>
public TEntity Update(TEntity entity)
    {
        throw new NotImplementedException();
        //return _repository.Update(entity);
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public Task<TEntity> UpdateAsync(TEntity entity)
    {
        return Task.FromResult(Update(entity));
    }

        /// <summary>
    /// GetRelated method.
    /// </summary>
public IEnumerable<TOut> GetRelated<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return _repository.GetRelated<TOut, TRelation>(id, relationship, predicate);
    }

        /// <summary>
    /// GetRelatedCount method.
    /// </summary>
public int GetRelatedCount<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return _repository.GetRelatedCount<TOut, TRelation>(id, relationship, predicate);
    }

        /// <summary>
    /// AddRelationShip method.
    /// </summary>
public bool AddRelationShip<TOut, TRelation>(TKey inboundId, TKey outboundId, string relationship, TRelation relation) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return _repository.AddRelationShip<TOut, TRelation>(inboundId, outboundId, relationship, relation);
    }

        /// <summary>
    /// HasRelationship method.
    /// </summary>
public bool HasRelationship<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return _repository.HasRelationship<TOut>(inboundId, outboundId, relationship);
    }

        /// <summary>
    /// GetRelatedAsync method.
    /// </summary>
public Task<IEnumerable<TOut>> GetRelatedAsync<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return Task.FromResult(GetRelated<TOut, TRelation>(id, relationship, predicate));
    }

        /// <summary>
    /// GetRelatedCountAsync method.
    /// </summary>
public Task<int> GetRelatedCountAsync<TOut, TRelation>(TKey id, string relationship, Expression<Func<TRelation, bool>> predicate) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return Task.FromResult(GetRelatedCount<TOut, TRelation>(id, relationship, predicate));
    }

        /// <summary>
    /// AddRelationShipAsync method.
    /// </summary>
public Task<bool> AddRelationShipAsync<TOut, TRelation>(TKey inboundId, TKey outboundId, string relationship, TRelation relation) where TOut : class, IEntity<TKey>, IEquatable<TKey> where TRelation : class
    {
        return Task.FromResult(AddRelationShip<TOut, TRelation>(inboundId, outboundId, relationship, relation));
    }

        /// <summary>
    /// HasRelationshipAsync method.
    /// </summary>
public Task<bool> HasRelationshipAsync<TOut>(TKey inboundId, TKey outboundId, string relationship) where TOut : class, IEntity<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(HasRelationship<TOut>(inboundId, outboundId, relationship));
    }

        /// <summary>
    /// Get method.
    /// </summary>
public IEnumerable<TEntity> Get(string label, int limit, int skip)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string label, Expression<Func<TEntity, bool>> predicate, int limit, int skip)
    {
        return _repository.Find(label, predicate, limit, skip);
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string label, string expression, int limit, int skip)
    {
        return _repository.Find(label, expression, limit, skip);
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IQueryable<TEntity> Find(string expression, int limit, int skip)
    {
        throw new NotImplementedException();
        // return _repository.Find(expression, limit, skip);
    }

        /// <summary>
    /// GetAsync method.
    /// </summary>
public Task<IEnumerable<TEntity>> GetAsync(string label, int limit, int skip)
    {
        return Task.FromResult(Get(label, limit, skip));
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public Task<IQueryable<TEntity>> FindAsync(string label, Expression<Func<TEntity, bool>> predicate, int limit, int skip)
    {
        return Task.FromResult(Find(label, predicate, limit, skip));
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public Task<IQueryable<TEntity>> FindAsync(string label, string expression, int limit, int skip)
    {
        return Task.FromResult(Find(label, expression, limit, skip));
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public Task<IQueryable<TEntity>> FindAsync(string expression, int limit, int skip)
    {
        return Task.FromResult(Find(expression, limit, skip));
    }
}