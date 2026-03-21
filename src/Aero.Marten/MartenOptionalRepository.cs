using Aero.Core.Entities;
using Microsoft.Extensions.Logging;
using Aero.Core.Data.Functional;

namespace Aero.Marten.Optional;

public interface IMartenReadonlyRepositorySyncOption<T, TKey> 
    : IReadonlyRepositorySyncOption<T,TKey> 
    where TKey : IEquatable<TKey>
    where T : IEntity<TKey>;

public interface IMartenReadonlyRepositoryAsyncOption<T, TKey>
    : IReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>;

public interface IMartenReadOnlyRepositoryOption<T, TKey>
    : IMartenReadonlyRepositorySyncOption<T, TKey>, IMartenReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenWriteOnlyRepositorySyncOption<T, TKey> 
    : IWriteOnlyRepositorySyncOption<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>;


public interface IMartenWriteOnlyRepositoryAsyncOption<T, TKey> 
    : IWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenWriteOnlyRepositoryOption<T, TKey>
    : IMartenWriteOnlyRepositorySyncOption<T, TKey>, IMartenWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenGenericRepositoryOption<T, TKey>
    : IMartenReadOnlyRepositoryOption<T, TKey>, IMartenWriteOnlyRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>;

/// <summary>
/// The main Generic repository for interface for implementing generic repositories.
/// This is for the main database used by the application the majority of the time. If
/// any specific repository is needed, don't swap the DI registration for this. Create a new
/// DI registration for the specific interface & concrete implementation.
/// </summary>
/// <typeparam name="T">The type of data model to be operated upon <see cref="IEntity{TKey}"/></typeparam>
/// <remarks>long is the default type for the primary key due to the Aero use of the snowflake algorithm</remarks>
public interface IMartenGenericRepositoryOption<T> : IMartenGenericRepositoryOption<T, long> where T : IEntity<long>, new();

public abstract class MartenGenericRepositoryOption<T>(ILogger<MartenGenericRepositoryOption<T>> log)
    : MartenGenericRepositoryOption<T, long>(log), IMartenGenericRepositoryOption<T>
    where T : IEntity<long>, new();

public abstract class MartenGenericRepositoryOption<T, TKey>(ILogger<MartenGenericRepositoryOption<T, TKey>> log)
    : GenericRepositoryOption<T, TKey>(log), IMartenGenericRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>;