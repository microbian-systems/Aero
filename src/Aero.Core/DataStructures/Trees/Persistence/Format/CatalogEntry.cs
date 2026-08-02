using System.Runtime.InteropServices;

namespace Aero.Core.DataStructures.Trees.Persistence.Format;

/// <summary>
/// Represents a struct for CatalogEntry.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CatalogEntry
{
        /// <summary>
    /// PageLsn.
    /// </summary>
public ulong PageLsn;
        /// <summary>
    /// TransactionId.
    /// </summary>
public long TransactionId;
        /// <summary>
    /// TreeName.
    /// </summary>
public unsafe fixed byte TreeName[128];
        /// <summary>
    /// RootPageId.
    /// </summary>
public long RootPageId;
        /// <summary>
    /// TreeType.
    /// </summary>
public byte TreeType;
        /// <summary>
    /// KeyTypeCode.
    /// </summary>
public byte KeyTypeCode;
        /// <summary>
    /// ValueTypeCode.
    /// </summary>
public byte ValueTypeCode;
        /// <summary>
    /// PageSize.
    /// </summary>
public int PageSize;
        /// <summary>
    /// CreatedAtUtc.
    /// </summary>
public long CreatedAtUtc;
        /// <summary>
    /// EntryCount.
    /// </summary>
public long EntryCount;
        /// <summary>
    /// IsolationLevel.
    /// </summary>
public byte IsolationLevel;
        /// <summary>
    /// HeapFilePageId.
    /// </summary>
public long HeapFilePageId;
        /// <summary>
    /// IndexType.
    /// </summary>
public byte IndexType;

        /// <summary>
    /// TreeNameLength.
    /// </summary>
public const int TreeNameLength = 128;
        /// <summary>
    /// Size.
    /// </summary>
public const int Size = 8 + 8 + 128 + 8 + 1 + 1 + 1 + 4 + 8 + 8 + 1 + 8 + 1;

        /// <summary>
    /// SerializedSize.
    /// </summary>
public static readonly int SerializedSize = Marshal.SizeOf<CatalogEntry>();

        /// <summary>
    /// GetTreeName method.
    /// </summary>
public string GetTreeName()
    {
        unsafe
        {
            var bytes = new byte[TreeNameLength];
            for (int i = 0; i < TreeNameLength; i++)
            {
                bytes[i] = TreeName[i];
                if (bytes[i] == 0)
                {
                    Array.Resize(ref bytes, i);
                    break;
                }
            }
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }

        /// <summary>
    /// SetTreeName method.
    /// </summary>
public void SetTreeName(string name)
    {
        unsafe
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(name ?? string.Empty);
            var length = Math.Min(bytes.Length, TreeNameLength - 1);
            
            for (int i = 0; i < TreeNameLength; i++)
            {
                TreeName[i] = i < length ? bytes[i] : (byte)0;
            }
        }
    }
}
