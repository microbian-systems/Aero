using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class AiUsageLog : ISnowflakeEntity
{
    public ulong UserId { get; set; } 
    [MaxLength(8000)]
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public ulong Id { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
}