namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a record for Lsn.
/// </summary>
public readonly record struct Lsn(ulong Value) : IComparable<Lsn>
{
        /// <summary>
    /// Zero.
    /// </summary>
public static readonly Lsn Zero = new(0);
        /// <summary>
    /// MinValue.
    /// </summary>
public static readonly Lsn MinValue = new(1);
        /// <summary>
    /// MaxValue.
    /// </summary>
public static readonly Lsn MaxValue = new(ulong.MaxValue);

        /// <summary>
    /// Gets or sets the Is Null.
    /// </summary>
public bool IsNull => Value == 0;

        /// <summary>
    /// CompareTo method.
    /// </summary>
public int CompareTo(Lsn other) => Value.CompareTo(other.Value);

    public static bool operator <(Lsn a, Lsn b) => a.Value < b.Value;
    public static bool operator >(Lsn a, Lsn b) => a.Value > b.Value;
    public static bool operator <=(Lsn a, Lsn b) => a.Value <= b.Value;
    public static bool operator >=(Lsn a, Lsn b) => a.Value >= b.Value;

        /// <summary>
    /// Next method.
    /// </summary>
public Lsn Next() => new(Value + 1);

        /// <summary>
    /// ToString method.
    /// </summary>
public override string ToString() => $"LSN({Value})";
}
