using System.ComponentModel.DataAnnotations;
using Aero.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a class for AddressModel.
/// </summary>
public class AddressModel : Entity
{
        /// <summary>
    /// Gets or sets the User Id.
    /// </summary>
[MaxLength(256)]
    public string UserId { get; set; }
        /// <summary>
    /// Gets or sets the Address Line1.
    /// </summary>
[PersonalData]
    [MaxLength(128)]
    public string AddressLine1 { get; set; }
        /// <summary>
    /// Gets or sets the Address Line2.
    /// </summary>
[PersonalData]
    [MaxLength(128)]
    public string AddressLine2 { get; set; }
        /// <summary>
    /// Gets or sets the Address Line3.
    /// </summary>
[PersonalData]
    [MaxLength(128)]
    public string AddressLine3 { get; set; }
        /// <summary>
    /// Gets or sets the City.
    /// </summary>
[PersonalData]
    [MaxLength(128)]
    public string City { get; set; }
        /// <summary>
    /// Gets or sets the State.
    /// </summary>
[MaxLength(128)]
    public string State { get; set; }
        /// <summary>
    /// Gets or sets the State Code.
    /// </summary>
public string StateCode { get; set; }
        /// <summary>
    /// Gets or sets the Country.
    /// </summary>
[MaxLength(128)]
    public string Country { get; set; }
        /// <summary>
    /// Gets or sets the Country Code.
    /// </summary>
[MaxLength(5)]
    public string CountryCode { get; set; }
        /// <summary>
    /// Gets or sets the Postal Code.
    /// </summary>
[MaxLength(128)]
    public string PostalCode { get; set; }
        /// <summary>
    /// Gets or sets the Is Main.
    /// </summary>
public bool IsMain { get; set; }
        /// <summary>
    /// Gets or sets the Is Active.
    /// </summary>
public bool IsActive { get; set; }
        /// <summary>
    /// Gets or sets the Latitude.
    /// </summary>
[PersonalData]
    public double? Latitude { get; set; }
        /// <summary>
    /// Gets or sets the Longitude.
    /// </summary>
[PersonalData]
    public double? Longitude { get; set; }
}