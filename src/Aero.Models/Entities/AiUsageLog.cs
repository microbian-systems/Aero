using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for AiUsageLog.
/// </summary>
public class AiUsageLog : EntityBase<long>, ISnowflakeEntity
{
        /// <summary>
    /// Gets or sets the User Id.
    /// </summary>
public long UserId { get; set; } 
        /// <summary>
    /// Gets or sets the Provider.
    /// </summary>
[MaxLength(8000)]
    public string Provider { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Timestamp.
    /// </summary>
public DateTimeOffset Timestamp { get; set; }
}