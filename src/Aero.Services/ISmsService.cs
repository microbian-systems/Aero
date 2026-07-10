namespace Aero.Services;

/// <summary>
/// Defines an interface for ISmsService.
/// </summary>
public interface ISmsService
{
        /// <summary>
    /// SendSms method.
    /// </summary>
Task SendSms(string from, string to, string body);
}