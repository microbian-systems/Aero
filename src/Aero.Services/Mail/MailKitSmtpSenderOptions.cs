namespace Aero.Services.Mail;

/// <summary>
/// Connection settings for <see cref="MailKitSmtpSender"/>.
/// </summary>
public sealed class MailKitSmtpSenderOptions
{
    /// <summary>The SMTP server host name.</summary>
    public required string Host { get; init; }

    /// <summary>The SMTP server port.</summary>
    public required int Port { get; init; }

    /// <summary>The required SMTP transport security mode. STARTTLS is required by default.</summary>
    public MailKitSmtpTransportSecurity TransportSecurity { get; init; } = MailKitSmtpTransportSecurity.StartTlsRequired;

    /// <summary>Explicitly permits plaintext SMTP. Credentials are prohibited in this mode.</summary>
    public bool AllowInsecureTransport { get; init; }

    /// <summary>Optional SMTP user name. It must be supplied together with <see cref="Password"/>.</summary>
    public string? Username { get; init; }

    /// <summary>Optional SMTP password. It must be supplied together with <see cref="Username"/>.</summary>
    public string? Password { get; init; }

    /// <summary>Bounds the complete send attempt, including connection, authentication, delivery, and cleanup.</summary>
    public TimeSpan OperationTimeout { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>SMTP transport security modes supported by the direct MailKit sender.</summary>
public enum MailKitSmtpTransportSecurity
{
    /// <summary>Require STARTTLS after the initial SMTP handshake.</summary>
    StartTlsRequired,

    /// <summary>Use TLS from connection establishment.</summary>
    TlsOnConnect,

    /// <summary>Use unencrypted SMTP only when explicitly opted in.</summary>
    Plaintext
}
