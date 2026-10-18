using System.Net;
using System.Text;
using Aero.Services.Mail;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TUnit.Assertions;
using TUnit.Core;

namespace Aero.Services.Mail.Tests;

/// <summary>
/// Behavioral contract tests for the Aero-owned MailKit FluentEmail adapter.
/// These tests keep transport effects synthetic while asserting the MIME message
/// that would be transmitted by MailKit.
/// </summary>
public sealed class MailKitSenderBehaviorTests
{
    [Test]
    public async Task Async_send_preserves_addresses_unicode_subject_headers_priorities_and_mime_parts()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        await using var regular = new MemoryStream(Encoding.UTF8.GetBytes("regular proposal material"));
        await using var inline = new MemoryStream(Encoding.UTF8.GetBytes("inline logo material"));
        regular.Position = 3;
        inline.Position = 2;

        var email = new Email(null, sender)
            .SetFrom("sender@example.test", "Contractor AI")
            .To("contracting.officer@example.test", "Contracting Officer")
            .CC("reviewer@example.test", "Proposal Reviewer")
            .BCC("archive@example.test", "Archive")
            .ReplyTo("replies@example.test", "Proposal Desk")
            .Subject("Proposal for terminals – résumé")
            .Body("<strong>HTML proposal</strong>", true)
            .Header("X-Contract-Notice", "notice-123")
            .Attach(new Attachment
            {
                Filename = "proposal.txt",
                ContentType = "text/plain",
                Data = regular,
                ContentId = "proposal-content-id"
            })
            .Attach(new Attachment
            {
                Filename = "logo.png",
                ContentType = "image/png",
                Data = inline,
                IsInline = true,
                ContentId = "logo-content-id"
            });
        email.Data.Priority = Priority.High;

        var response = await email.SendAsync();
        var message = Parse(transport.SerializedMessage);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(transport.AsyncSendCalls).IsEqualTo(1);
        await Assert.That(transport.SyncSendCalls).IsEqualTo(0);
        await Assert.That(transport.ConnectSecurity).IsEqualTo(SecureSocketOptions.StartTls);
        await Assert.That(message.From.Mailboxes.Single().Address).IsEqualTo("sender@example.test");
        await Assert.That(message.To.Mailboxes.Single().Address).IsEqualTo("contracting.officer@example.test");
        await Assert.That(message.Cc.Mailboxes.Single().Address).IsEqualTo("reviewer@example.test");
        await Assert.That(message.Bcc.Mailboxes.Single().Address).IsEqualTo("archive@example.test");
        await Assert.That(message.ReplyTo.Mailboxes.Single().Address).IsEqualTo("replies@example.test");
        await Assert.That(message.Subject).IsEqualTo("Proposal for terminals – résumé");
        await Assert.That(message.Headers["X-Contract-Notice"]).IsEqualTo("notice-123");
        await Assert.That(message.Importance).IsEqualTo(MessageImportance.High);
        await Assert.That(message.HtmlBody).Contains("<strong>HTML proposal</strong>");
        await Assert.That(message.Attachments.OfType<MimePart>().Single(part => part.FileName == "proposal.txt").ContentId).IsEqualTo("proposal-content-id");
        await Assert.That(message.BodyParts.OfType<MimePart>().Single(part => part.FileName == "logo.png").ContentId).IsEqualTo("logo-content-id");
        await Assert.That(regular.CanRead).IsTrue();
        await Assert.That(regular.Position).IsEqualTo(3L);
        await Assert.That(inline.CanRead).IsTrue();
        await Assert.That(inline.Position).IsEqualTo(2L);
        await Assert.That(transport.Disposed).IsTrue();
    }

    [Test]
    public async Task Sync_send_uses_native_sync_transport_and_tls_on_connect()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport, MailKitSmtpTransportSecurity.TlsOnConnect);
        var email = NewEmail(sender).Body("plain body", false);
        email.Data.Priority = Priority.Low;

        var response = email.Send();
        var message = Parse(transport.SerializedMessage);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(transport.SyncSendCalls).IsEqualTo(1);
        await Assert.That(transport.AsyncSendCalls).IsEqualTo(0);
        await Assert.That(transport.ConnectSecurity).IsEqualTo(SecureSocketOptions.SslOnConnect);
        await Assert.That(message.TextBody).Contains("plain body");
        await Assert.That(message.HtmlBody).IsNull();
        await Assert.That(message.Importance).IsEqualTo(MessageImportance.Low);
        await Assert.That(transport.Disposed).IsTrue();
    }

    [Test]
    public async Task Plaintext_alternative_body_creates_both_text_and_html_representations()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        var email = NewEmail(sender).Body("HTML fallback", false);
        email.Data.PlaintextAlternativeBody = "Plain alternative";
        email.Data.Priority = Priority.Normal;

        var response = await email.SendAsync();
        var message = Parse(transport.SerializedMessage);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(message.TextBody).Contains("Plain alternative");
        await Assert.That(message.HtmlBody).Contains("HTML fallback");
        await Assert.That(message.Importance).IsEqualTo(MessageImportance.Normal);
    }

    [Test]
    public async Task Whitespace_only_plaintext_alternative_is_preserved_as_legacy_text_content()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        var email = NewEmail(sender).Body("HTML fallback", false);
        email.Data.PlaintextAlternativeBody = " \t ";

        var response = await email.SendAsync();
        var message = Parse(transport.SerializedMessage);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(message.TextBody).IsEqualTo(" \t ");
        await Assert.That(message.HtmlBody).Contains("HTML fallback");
    }

    [Test]
    public async Task Pre_cancelled_send_does_not_create_or_connect_a_transport()
    {
        var factory = new CountingFactory();
        var sender = CreateSender(factory);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var response = await NewEmail(sender).Body("body").SendAsync(cancellation.Token);

        await Assert.That(response.Successful).IsFalse();
        await Assert.That(response.ErrorMessages).Contains("Message was cancelled by cancellation token.");
        await Assert.That(factory.CreateCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Known_smtp_rejection_is_stable_and_redacted_on_both_sync_and_async_paths()
    {
        var sync = new CapturingTransport { SendException = Rejection() };
        var asyncTransport = new CapturingTransport { SendException = Rejection() };

        var syncResponse = NewEmail(CreateSender(sync)).Body("body").Send();
        var asyncResponse = await NewEmail(CreateSender(asyncTransport)).Body("body").SendAsync();

        await Assert.That(syncResponse.Successful).IsFalse();
        await Assert.That(asyncResponse.Successful).IsFalse();
        await Assert.That(syncResponse.ErrorMessages).Contains("SMTP server rejected the message.");
        await Assert.That(asyncResponse.ErrorMessages).Contains("SMTP server rejected the message.");
        await Assert.That(syncResponse.ErrorMessages.Any(message => message.Contains("secret@example.test", StringComparison.Ordinal))).IsFalse();
        await Assert.That(asyncResponse.ErrorMessages.Any(message => message.Contains("secret@example.test", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task Cancellation_or_transport_failure_after_send_starts_reports_unknown_delivery_outcome()
    {
        var cancelled = new CapturingTransport { SendException = new OperationCanceledException("sensitive transport detail") };
        var failed = new CapturingTransport { SendException = new InvalidOperationException("smtp password leaked") };

        var cancelledResponse = await NewEmail(CreateSender(cancelled)).Body("body").SendAsync();
        var failedResponse = NewEmail(CreateSender(failed)).Body("body").Send();

        await Assert.That(cancelledResponse.ErrorMessages).Contains("Email delivery outcome is unknown.");
        await Assert.That(failedResponse.ErrorMessages).Contains("Email delivery outcome is unknown.");
        await Assert.That(cancelledResponse.ErrorMessages.Any(message => message.Contains("sensitive", StringComparison.OrdinalIgnoreCase))).IsFalse();
        await Assert.That(failedResponse.ErrorMessages.Any(message => message.Contains("password", StringComparison.OrdinalIgnoreCase))).IsFalse();
    }

    [Test]
    public async Task Accepted_message_remains_successful_when_quit_fails()
    {
        var transport = new CapturingTransport { DisconnectException = new IOException("QUIT failed") };

        var response = await NewEmail(CreateSender(transport)).Body("body").SendAsync();

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(response.MessageId).IsEqualTo("synthetic-message-id");
        await Assert.That(transport.Disposed).IsTrue();
    }

    private static Email NewEmail(MailKitSmtpSender sender)
    {
        var email = new Email(null, sender);
        email.SetFrom("sender@example.test", "Sender")
            .To("recipient@example.test", "Recipient")
            .Subject("subject");
        return email;
    }

    private static MailKitSmtpSender CreateSender(CapturingTransport transport, MailKitSmtpTransportSecurity security = MailKitSmtpTransportSecurity.StartTlsRequired) =>
        CreateSender(new FixedFactory(transport), security);

    private static MailKitSmtpSender CreateSender(IMailKitSmtpClientFactory factory, MailKitSmtpTransportSecurity security = MailKitSmtpTransportSecurity.StartTlsRequired) => new(
        new MailKitSmtpSenderOptions { Host = "smtp.example.test", Port = 587, TransportSecurity = security }, factory);

    private static SmtpCommandException Rejection() => new(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.TransactionFailed, "secret@example.test rejected");

    private static MimeMessage Parse(byte[] serialized)
    {
        using var stream = new MemoryStream(serialized, writable: false);
        return MimeMessage.Load(stream);
    }

    private sealed class FixedFactory(CapturingTransport transport) : IMailKitSmtpClientFactory
    {
        public IMailKitSmtpClient Create() => transport;
    }

    private sealed class CountingFactory : IMailKitSmtpClientFactory
    {
        public int CreateCalls { get; private set; }
        public IMailKitSmtpClient Create()
        {
            CreateCalls++;
            return new CapturingTransport();
        }
    }

    private sealed class CapturingTransport : IMailKitSmtpClient
    {
        public byte[] SerializedMessage { get; private set; } = [];
        public SecureSocketOptions? ConnectSecurity { get; private set; }
        public int SyncSendCalls { get; private set; }
        public int AsyncSendCalls { get; private set; }
        public bool Disposed { get; private set; }
        public Exception? SendException { get; init; }
        public Exception? DisconnectException { get; init; }

        public void Connect(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken) => ConnectSecurity = security;
        public Task ConnectAsync(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken)
        {
            ConnectSecurity = security;
            return Task.CompletedTask;
        }
        public void Authenticate(ICredentials credentials, CancellationToken cancellationToken) { }
        public Task AuthenticateAsync(ICredentials credentials, CancellationToken cancellationToken) => Task.CompletedTask;
        public string Send(MimeMessage message, CancellationToken cancellationToken)
        {
            SyncSendCalls++;
            Capture(message);
            if (SendException is not null) throw SendException;
            return "synthetic-message-id";
        }
        public Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            AsyncSendCalls++;
            Capture(message);
            if (SendException is not null) return Task.FromException<string>(SendException);
            return Task.FromResult("synthetic-message-id");
        }
        public void Disconnect(bool quit, CancellationToken cancellationToken)
        {
            if (DisconnectException is not null) throw DisconnectException;
        }
        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
        {
            if (DisconnectException is not null) return Task.FromException(DisconnectException);
            return Task.CompletedTask;
        }
        public void Dispose() => Disposed = true;
        private void Capture(MimeMessage message)
        {
            using var stream = new MemoryStream();
            message.WriteTo(stream);
            SerializedMessage = stream.ToArray();
        }
    }
}
