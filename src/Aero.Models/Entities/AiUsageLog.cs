using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class AiUsageLog : EntityBase<ulong>, ISnowflakeEntity
{
    public ulong UserId { get; set; } 
    [MaxLength(8000)]
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}