namespace Aero.Core.Entities;

/// <summary>
/// Defines an interface for IEntityInt.
/// </summary>
public interface IEntityInt : IEntity<int>;

/// <summary>
/// Defines an interface for IEntityString.
/// </summary>
public interface IEntityString : IEntity<string>;

/// <summary>
/// Defines an interface for IEntityGuid.
/// </summary>
public interface IEntityGuid : IEntity<Guid>;

/// <summary>
/// Defines an interface for IEntityLong.
/// </summary>
public interface IEntityLong : IEntity<long>;

/// <summary>
/// Defines an interface for ISnowflakeEntity.
/// </summary>
public interface ISnowflakeEntity : IEntityLong;