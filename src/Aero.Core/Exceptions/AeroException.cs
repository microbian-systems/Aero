namespace Aero.Core.Exceptions;

/// <summary>
/// Base Exception for errors arising from with the Aero library
/// </summary>
public class AeroException(string message) : Exception(message);