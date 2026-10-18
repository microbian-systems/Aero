using FluentValidation;

namespace Aero.Services.Mail;

/// <summary>Validates the security-sensitive SMTP transport settings before a client is created.</summary>
public sealed class MailKitSmtpSenderOptionsValidator : AbstractValidator<MailKitSmtpSenderOptions>
{
    /// <summary>Initializes validation rules for the direct MailKit sender.</summary>
    public MailKitSmtpSenderOptionsValidator()
    {
        RuleFor(options => options.Host).NotEmpty();
        RuleFor(options => options.Port).InclusiveBetween(1, 65535);
        RuleFor(options => options.OperationTimeout).GreaterThan(TimeSpan.Zero).LessThanOrEqualTo(TimeSpan.FromMinutes(5));
        RuleFor(options => options.TransportSecurity).IsInEnum();
        RuleFor(options => options.AllowInsecureTransport)
            .Equal(true)
            .When(options => options.TransportSecurity == MailKitSmtpTransportSecurity.Plaintext)
            .WithMessage("Plaintext SMTP requires an explicit insecure-transport opt-in.");
        RuleFor(options => options.Password)
            .NotEmpty()
            .When(options => !string.IsNullOrWhiteSpace(options.Username));
        RuleFor(options => options.Username)
            .NotEmpty()
            .When(options => !string.IsNullOrWhiteSpace(options.Password));
        RuleFor(options => options.Username)
            .Empty()
            .When(options => options.TransportSecurity == MailKitSmtpTransportSecurity.Plaintext)
            .WithMessage("SMTP credentials cannot be sent over plaintext transport.");
    }
}
