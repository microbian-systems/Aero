namespace Aero.Core.Security;

/// <summary>
/// Defines an interface for IHtmlSanitizer.
/// </summary>
public interface IHtmlSanitizer
{
        /// <summary>
    /// Sanitize method.
    /// </summary>
string Sanitize(string? html);
}
