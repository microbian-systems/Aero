namespace Aero.MartenDB.Identity;

/// <summary>
/// Specifies the type of ID used for users.
/// </summary>
public enum UserIdType
{
    /// <summary>
    /// User has specified their own ID.
    /// </summary>
    None,

    /// <summary>
    /// Use the user's email address as their ID.
    /// </summary>
    Email,

    /// <summary>
    /// Use the user's username as their ID.
    /// </summary>
    UserName,

    /// <summary>
    /// Use consecutive IDs (e.g. users/1, users/2, etc.).
    /// </summary>
    Consecutive,

    /// <summary>
    /// Use number-tag IDs (e.g. users/1-a, users/2-b, etc.).
    /// </summary>
    NumberTag
}
