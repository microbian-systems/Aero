using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class AiUsageLog : EntityBase<long>, ISnowflakeEntity
{
    public long UserId { get; set; } 
    [MaxLength(8000)]
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}