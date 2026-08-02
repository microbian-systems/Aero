namespace Aero.Core.DataStructures.Trees.Persistence.Format;

/// <summary>
/// Represents a class for UnsupportedFormatVersionException.
/// </summary>
public sealed class UnsupportedFormatVersionException(ushort found, ushort supported)
    : Exception($"File format version {found} is not supported. Maximum supported: {supported}.")
{
        /// <summary>
    /// Gets or sets the Found Version.
    /// </summary>
public ushort FoundVersion { get; } = found;
        /// <summary>
    /// Gets or sets the Supported Version.
    /// </summary>
public ushort SupportedVersion { get; } = supported;
}
