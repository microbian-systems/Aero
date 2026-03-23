using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aero.Common.Web.Email;

public class FluentEmailSender(IFluentEmail client, ILogger<FluentEmailSender> log) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        log.LogInformation($"sending email to {email}");
        await client.To(email)
            .Subject(subject)
            .Body(htmlMessage, true)
            .SendAsync();
        log.LogInformation($"email successfully sent");
    }
}