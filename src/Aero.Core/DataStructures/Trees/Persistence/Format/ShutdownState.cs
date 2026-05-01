namespace Aero.Core.DataStructures.Trees.Persistence.Format;

public enum ShutdownState : byte
{
    Clean = 0x01,
    Dirty = 0x02,
}
