namespace Aero.Modular;

// todo - abstract/extract Aero modules into its own lib so it can be used in any type of app (host, console, web, etc)


/// <summary>
/// Exception thrown when the module system fails during startup.
/// </summary>
public class ModuleSystemStartupException : Exception
{
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleSystemStartupException"/> class.
    /// </summary>
public ModuleSystemStartupException(string message) : base(message) { }
        /// <summary>
    /// Initializes a new instance of the <see cref="ModuleSystemStartupException"/> class.
    /// </summary>
public ModuleSystemStartupException(string message, Exception inner) : base(message, inner) { }
}
