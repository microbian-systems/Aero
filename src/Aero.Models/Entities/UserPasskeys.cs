using System.ComponentModel.DataAnnotations.Schema;
using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for UserPasskeys.
/// </summary>
[Table("UserPasskeys")]
public class UserPasskeys : Entity
{
        /// <summary>
    /// Gets or sets the User Id.
    /// </summary>
public string UserId { get; set; } = string.Empty;    
        /// <summary>
    /// Gets or sets the Credential Id.
    /// </summary>
public byte[] CredentialId { get; set; }    
        /// <summary>
    /// Gets or sets the Public Key.
    /// </summary>
public byte[] PublicKey { get; set; }    
        /// <summary>
    /// Gets or sets the Signature Counter.
    /// </summary>
public uint SignatureCounter { get; set; }
}
