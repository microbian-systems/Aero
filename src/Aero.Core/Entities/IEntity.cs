using System.ComponentModel.DataAnnotations;

namespace Aero.Core.Entities;

/// <summary>
/// Defines an interface for IEntity.
/// </summary>
public interface IEntity : ISnowflakeEntity;
/// <summary>
/// Defines an interface for IEntity.
/// </summary>
public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[Key] [JsonPropertyName("id")]
    TKey Id { get; set; }

        /// <summary>
    /// Gets or sets the Created On.
    /// </summary>
[JsonPropertyName("created_on")]
    public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
    /// Gets or sets the Modified On.
    /// </summary>
[JsonPropertyName("modified_on")]
    public DateTimeOffset? ModifiedOn { get; set; }

        /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
[JsonPropertyName("created_by")]
    public string CreatedBy { get; set; }

        /// <summary>
    /// Gets or sets the Modified By.
    /// </summary>
[JsonPropertyName("modified_by")]
    public string ModifiedBy { get; set; }
}

/// <summary>
/// Represents a persisted entity for Aero
/// </summary>
public abstract class Entity : Entity<long>, IEntity {}

/// <summary>
/// Represents a persisted entity for Aero
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class Entity<TKey> : EntityBase<TKey> where TKey : IEquatable<TKey>
{
}

/// <summary>
/// Represents an enetity that can be persisted
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class EntityBase<TKey> : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[Key]
    [JsonPropertyName("id")]
    public TKey Id { get; set; }

        /// <summary>
    /// Gets or sets the Created On.
    /// </summary>
[JsonPropertyName("created_on")]
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
    /// Gets or sets the Modified On.
    /// </summary>
[JsonPropertyName("modified_on")]
    public DateTimeOffset? ModifiedOn { get; set; }

        /// <summary>
    /// Gets or sets the Created By.
    /// </summary>
[JsonPropertyName("created_by")]
    public string CreatedBy { get; set; }

        /// <summary>
    /// Gets or sets the Modified By.
    /// </summary>
[JsonPropertyName("updated_by")]
    public string ModifiedBy { get; set; }
}