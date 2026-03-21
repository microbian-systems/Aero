namespace Aero.Core.Entities;

public interface IEntityInt : IEntity<int>;

public interface IEntityString : IEntity<string>;

public interface IEntityGuid : IEntity<Guid>;

public interface IEntityLong : IEntity<long>;

public interface ISnowflakeEntity : IEntityLong;