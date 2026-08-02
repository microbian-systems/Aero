using System.Security.Cryptography;
using FlakeId;

namespace Aero.Core;


/// <summary>
/// Twitter snowflake unique id generator class
/// </summary>
public static class Snowflake
{
    private const long BrowserEpochMilliseconds = 1_577_836_800_000L;
    private const int BrowserSequenceBits = 12;
    private const int BrowserMachineBits = 10;
    private const int BrowserSequenceMask = (1 << BrowserSequenceBits) - 1;
    private static readonly object BrowserIdLock = new();
    private static long _lastBrowserTimestamp;
    private static int _browserSequence;

    // todo - figure out how to set the machine id at runtime (use asynclocks (dotnext) and redis/garnet for scaleout situations)
        /// <summary>
    /// Gets or sets the Machine Id.
    /// </summary>
public static int MachineId { get; private set; } = RandomNumberGenerator.GetInt32(1, 1024);
        /// <summary>
    /// SetMachineId method.
    /// </summary>
public static void SetMachineId(int machineId) => MachineId = machineId;

    static Snowflake()
    {
        
    }


    /// <summary>
    /// /Gets a new snowflake id
    /// </summary>
    /// <returns>snowflake id of type long</returns>
    public static long NewId()
    {
        if (OperatingSystem.IsBrowser())
        {
            return NewBrowserId();
        }

        var snowflake = Id.Create();
        return snowflake; // for ef core / db reasons
    }

    private static long NewBrowserId()
    {
        lock (BrowserIdLock)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - BrowserEpochMilliseconds;
            if (timestamp < 0)
            {
                throw new InvalidOperationException("The system clock predates the Aero snowflake epoch.");
            }

            if (timestamp > _lastBrowserTimestamp)
            {
                _lastBrowserTimestamp = timestamp;
                _browserSequence = 0;
            }
            else
            {
                timestamp = _lastBrowserTimestamp;
                _browserSequence = (_browserSequence + 1) & BrowserSequenceMask;
                if (_browserSequence == 0)
                {
                    timestamp = ++_lastBrowserTimestamp;
                }
            }

            return (timestamp << (BrowserMachineBits + BrowserSequenceBits))
                | ((long)MachineId << BrowserSequenceBits)
                | (uint)_browserSequence;
        }
    }
}
