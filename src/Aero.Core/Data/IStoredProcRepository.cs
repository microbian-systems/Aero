namespace Aero.Core.Data;

/// <summary>
/// Defines an interface for IStoredProcRepository.
/// </summary>
public interface IStoredProcRepository
{
        /// <summary>
    /// ExecStoredProc method.
    /// </summary>
void ExecStoredProc(string name, params object[] parameters);
        /// <summary>
    /// ExecStoredProc method.
    /// </summary>
object ExecStoredProc<U>(string name, params object[] parameters);
        /// <summary>
    /// ExecStoredProcAsync method.
    /// </summary>
Task ExecStoredProcAsync(string name, params object[] parameters);
        /// <summary>
    /// ExecStoredProcAsync method.
    /// </summary>
Task<object> ExecStoredProcAsync<U>(string name, params object[] parameters);
}