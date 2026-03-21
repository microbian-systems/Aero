using System.Security.Cryptography;
using FlakeId;
using FlakeId.Extensions;

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
        
    }


    /// <summary>
    /// /Gets a new snowflake id
    /// </summary>
    /// <returns>snowflake id of type long</returns>
    public static long NewId()
    {
        var snowflake = Id.Create();
        return snowflake; // for ef core / db reasons
    }
}