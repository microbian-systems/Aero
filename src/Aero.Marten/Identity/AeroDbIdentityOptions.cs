namespace Aero.MartenDB.Identity;

/// <summary>
/// Options for configuring AeroDB-based identity.
/// </summary>
public class AeroDbIdentityOptions
{
    /// <summary>
    /// Gets or sets the type of user ID to use.
    /// </summary>
    public UserIdType UserIdType { get; set; } = UserIdType.None;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically save changes to the database.
    /// </summary>
    public bool AutoSaveChanges { get; set; } = true;
}
