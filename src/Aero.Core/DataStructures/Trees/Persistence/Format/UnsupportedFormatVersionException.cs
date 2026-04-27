namespace Aero.Core.DataStructures.Trees.Persistence.Format;

public sealed class UnsupportedFormatVersionException(ushort found, ushort supported)
    : Exception($"File format version {found} is not supported. Maximum supported: {supported}.")
{
    public ushort FoundVersion { get; } = found;
    public ushort SupportedVersion { get; } = supported;
}
