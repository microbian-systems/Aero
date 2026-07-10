namespace Aero.Core.DataStructures.Trees.Persistence.Format;

/// <summary>
/// Defines an enumeration for ShutdownState.
/// </summary>
public enum ShutdownState : byte
{
    Clean = 0x01,
    Dirty = 0x02,
}
