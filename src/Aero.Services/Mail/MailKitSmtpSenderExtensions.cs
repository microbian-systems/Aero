using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aero.Services.Mail;

/// <summary>Registers the direct MailKit FluentEmail sender.</summary>
public static class MailKitSmtpSenderExtensions
{
    /// <summary>Registers a scoped direct MailKit sender for FluentEmail.</summary>
    public static FluentEmailServicesBuilder AddMailKitSmtpSender(
        this FluentEmailServicesBuilder builder,
        MailKitSmtpSenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.AddMailKitSmtpSender(_ => options);
    }

    /// <summary>Registers a scoped sender whose options are resolved for the current service scope.</summary>
    public static FluentEmailServicesBuilder AddMailKitSmtpSender(
        this FluentEmailServicesBuilder builder,
        Func<IServiceProvider, MailKitSmtpSenderOptions> optionsFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(optionsFactory);
        builder.Services.TryAddSingleton<IMailKitSmtpClientFactory, MailKitSmtpClientFactory>();
        builder.Services.Replace(ServiceDescriptor.Scoped<ISender>(sp => new MailKitSmtpSender(optionsFactory(sp), sp.GetRequiredService<IMailKitSmtpClientFactory>())));
        return builder;
    }
}
