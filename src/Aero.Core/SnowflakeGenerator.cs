using System.Security.Cryptography;

namespace Aero.Core;


/// <summary>
/// Twitter snowflake unique id generator class
/// </summary>
public static class Snowflake
{
    // todo - figure out how to set the machine id at runtime (use asynclocks (dotnext) and redis/garnet for scaleout situations)
    public static int MachineId { get; private set; } = RandomNumberGenerator.GetInt32(1, 1024);
    public static void SetMachineId(int machineId) => MachineId = machineId;

    static Snowflake()
    {
        SnowflakeGuid.SetMachineID(MachineId);
    }


    /// <summary>
    /// /Gets a new snowflake id
    /// </summary>
    /// <returns>snowflake id of type ulong</returns>
    public static ulong NewId()
    {
        var snowflake = SnowflakeGuid.Create();
        return snowflake.Id; // for ef core / db reasons
    }

    /// <summary>
    /// Sets the id of the snowflake generator machine
    /// </summary>
    /// <param name="machineId">the numeric id of the snowflake generation machine</param>
    /// <exception cref="ArgumentException">machineId cannot be greater than 1023</exception>
    static void SetMachineId(ushort machineId)
    {
        if (machineId > 1023) throw new ArgumentException($"{nameof(machineId)} cannot be greater than 1023");
        SnowflakeGuid.SetMachineID(machineId);
    }
}