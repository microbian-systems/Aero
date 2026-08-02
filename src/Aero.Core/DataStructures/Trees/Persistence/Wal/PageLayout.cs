namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a class for PageLayout.
/// </summary>
public static class PageLayout
{
        /// <summary>
    /// PageLsnOffset.
    /// </summary>
public const int PageLsnOffset = 0;
        /// <summary>
    /// PageLsnLength.
    /// </summary>
public const int PageLsnLength = sizeof(ulong);
        /// <summary>
    /// PageVersionOffset.
    /// </summary>
public const int PageVersionOffset = 8;
        /// <summary>
    /// NodeTypeOffset.
    /// </summary>
public const int NodeTypeOffset = 12;
        /// <summary>
    /// HeaderSize.
    /// </summary>
public const int HeaderSize = 16;
}
