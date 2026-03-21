using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

public class ApiClaimsModel : Entity
{
    [MaxLength(128)]
    public required string ClaimKey { get; set; }
    [MaxLength(1024)]
    public required string ClaimValue { get; set; }
    
    public long AccountId { get; set; }
}