using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aero.Web.Email;

/// <summary>
/// Represents a class for MailtrapEmailSender.
/// </summary>
public class MailtrapEmailSender : IEmailSender
{
        /// <summary>
    /// SendEmailAsync method.
    /// </summary>
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await Task.Delay(0);
    }
}