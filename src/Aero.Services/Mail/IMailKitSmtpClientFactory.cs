using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Aero.Services.Mail;

/// <summary>Creates a short-lived SMTP client for one send operation.</summary>
public interface IMailKitSmtpClientFactory
{
    /// <summary>Creates a transport client.</summary>
    IMailKitSmtpClient Create();
}

/// <summary>Small adapter boundary around MailKit transport operations.</summary>
public interface IMailKitSmtpClient : IDisposable
{
    /// <summary>Connects synchronously.</summary>
    void Connect(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken);
    /// <summary>Connects asynchronously.</summary>
    Task ConnectAsync(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken);
    /// <summary>Authenticates synchronously.</summary>
    void Authenticate(ICredentials credentials, CancellationToken cancellationToken);
    /// <summary>Authenticates asynchronously.</summary>
    Task AuthenticateAsync(ICredentials credentials, CancellationToken cancellationToken);
    /// <summary>Sends synchronously.</summary>
    string Send(MimeMessage message, CancellationToken cancellationToken);
    /// <summary>Sends asynchronously.</summary>
    Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken);
    /// <summary>Disconnects synchronously.</summary>
    void Disconnect(bool quit, CancellationToken cancellationToken);
    /// <summary>Disconnects asynchronously.</summary>
    Task DisconnectAsync(bool quit, CancellationToken cancellationToken);
}

/// <summary>Default MailKit transport factory retaining MailKit certificate validation defaults.</summary>
public sealed class MailKitSmtpClientFactory : IMailKitSmtpClientFactory
{
    /// <inheritdoc />
    public IMailKitSmtpClient Create() => new MailKitSmtpClientAdapter();
    private sealed class MailKitSmtpClientAdapter : IMailKitSmtpClient
    {
        private readonly SmtpClient client = new();
        public void Connect(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken) => client.Connect(host, port, security, cancellationToken);
        public Task ConnectAsync(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken) => client.ConnectAsync(host, port, security, cancellationToken);
        public void Authenticate(ICredentials credentials, CancellationToken cancellationToken) => client.Authenticate(credentials, cancellationToken);
        public Task AuthenticateAsync(ICredentials credentials, CancellationToken cancellationToken) => client.AuthenticateAsync(credentials, cancellationToken);
        public string Send(MimeMessage message, CancellationToken cancellationToken) => client.Send(message, cancellationToken);
        public Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken) => client.SendAsync(message, cancellationToken);
        public void Disconnect(bool quit, CancellationToken cancellationToken) => client.Disconnect(quit, cancellationToken);
        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken) => client.DisconnectAsync(quit, cancellationToken);
        public void Dispose() => client.Dispose();
    }
}
