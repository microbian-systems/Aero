namespace Aero.Core.DataStructures.Trees.Persistence.Format;

/// <summary>
/// Defines an enumeration for TypeCode.
/// </summary>
public enum TypeCode : byte
{
    Int32 = 0x01,
    Int64 = 0x02,
    Guid = 0x03,
    Single = 0x04,
    Double = 0x05,
    StringKey32 = 0x10,
    StringKey64 = 0x11,
    StringKey128 = 0x12,
    StringKey256 = 0x13,
}
