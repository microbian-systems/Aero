using System.Net;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using FluentValidation;

namespace Aero.Services.Mail;

/// <summary>
/// A FluentEmail sender implemented directly on the supported MailKit and MimeKit packages.
/// </summary>
public sealed class MailKitSmtpSender : ISender
{
    private readonly MailKitSmtpSenderOptions options;
    private readonly IMailKitSmtpClientFactory clientFactory;

    /// <summary>Creates a sender with validated SMTP connection settings.</summary>
    public MailKitSmtpSender(MailKitSmtpSenderOptions options, IMailKitSmtpClientFactory? clientFactory = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        new MailKitSmtpSenderOptionsValidator().ValidateAndThrow(options);

        this.options = new MailKitSmtpSenderOptions
        {
            Host = options.Host,
            Port = options.Port,
            TransportSecurity = options.TransportSecurity,
            AllowInsecureTransport = options.AllowInsecureTransport,
            Username = options.Username,
            Password = options.Password,
            OperationTimeout = options.OperationTimeout
        };
        this.clientFactory = clientFactory ?? new MailKitSmtpClientFactory();
    }

    /// <inheritdoc />
    public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
    {
        ArgumentNullException.ThrowIfNull(email);
        var cancellationToken = token ?? CancellationToken.None;
        if (cancellationToken.IsCancellationRequested) return CancelledResponse();
        var sendStarted = false;
        try
        {
            using var message = CreateMessage(email.Data);
            using var client = clientFactory.Create();
            using var timeout = CreateTimeout(cancellationToken);
            client.Connect(options.Host, options.Port, GetSocketOptions(), timeout.Token);
            if (!string.IsNullOrWhiteSpace(options.Username)) client.Authenticate(new NetworkCredential(options.Username, options.Password), timeout.Token);
            sendStarted = true;
            var id = client.Send(message, timeout.Token);
            try { client.Disconnect(true, timeout.Token); } catch { }
            return new SendResponse { MessageId = id };
        }
        catch (OperationCanceledException) when (!sendStarted) { return CancelledResponse(); }
        catch (SmtpCommandException) when (sendStarted) { return new SendResponse { ErrorMessages = ["SMTP server rejected the message."] }; }
        catch (Exception) { return new SendResponse { ErrorMessages = [sendStarted ? "Email delivery outcome is unknown." : "Unable to send email."] }; }
    }

    /// <inheritdoc />
    public async Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
    {
        ArgumentNullException.ThrowIfNull(email);
        var cancellationToken = token ?? CancellationToken.None;
        if (cancellationToken.IsCancellationRequested)
            return CancelledResponse();

        var sendAttemptStarted = false;

        try
        {
            using var message = CreateMessage(email.Data);
            using var client = clientFactory.Create();
            using var timeout = CreateTimeout(cancellationToken);

            await client.ConnectAsync(options.Host, options.Port, GetSocketOptions(), timeout.Token).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                await client.AuthenticateAsync(
                    new NetworkCredential(options.Username, options.Password), timeout.Token).ConfigureAwait(false);
            }

            sendAttemptStarted = true;
            var messageId = await client.SendAsync(message, timeout.Token).ConfigureAwait(false);
            try { await client.DisconnectAsync(true, timeout.Token).ConfigureAwait(false); }
            catch { /* The SMTP server accepted the message; QUIT failure does not change delivery outcome. */ }
            return new SendResponse { MessageId = messageId };
        }
        catch (OperationCanceledException) when (!sendAttemptStarted)
        {
            return CancelledResponse();
        }
        catch (SmtpCommandException) when (sendAttemptStarted)
        {
            return new SendResponse { ErrorMessages = ["SMTP server rejected the message."] };
        }
        catch (Exception) when (sendAttemptStarted)
        {
            return new SendResponse { ErrorMessages = ["Email delivery outcome is unknown."] };
        }
        catch (Exception)
        {
            return new SendResponse { ErrorMessages = ["Unable to send email."] };
        }
    }

    private CancellationTokenSource CreateTimeout(CancellationToken cancellationToken)
    {
        var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.OperationTimeout);
        return timeout;
    }

    private static SendResponse CancelledResponse() =>
        new() { ErrorMessages = ["Message was cancelled by cancellation token."] };

    private SecureSocketOptions GetSocketOptions() => options.TransportSecurity switch
    {
        MailKitSmtpTransportSecurity.StartTlsRequired => SecureSocketOptions.StartTls,
        MailKitSmtpTransportSecurity.TlsOnConnect => SecureSocketOptions.SslOnConnect,
        MailKitSmtpTransportSecurity.Plaintext => SecureSocketOptions.None,
        _ => throw new InvalidOperationException("Unsupported SMTP transport security mode.")
    };

    private static MimeMessage CreateMessage(EmailData email)
    {
        var message = new MimeMessage
        {
            Subject = email.Subject ?? string.Empty,
            Importance = email.Priority switch
            {
                Priority.High => MessageImportance.High,
                Priority.Low => MessageImportance.Low,
                _ => MessageImportance.Normal
            }
        };

        AddAddresses(message.From, email.FromAddress is null ? [] : [email.FromAddress]);
        AddAddresses(message.To, email.ToAddresses);
        AddAddresses(message.Cc, email.CcAddresses);
        AddAddresses(message.Bcc, email.BccAddresses);
        AddAddresses(message.ReplyTo, email.ReplyToAddresses);

        foreach (var (name, value) in email.Headers)
            message.Headers[name] = value;

        var body = new BodyBuilder();
        if (!string.IsNullOrEmpty(email.PlaintextAlternativeBody))
        {
            body.HtmlBody = email.Body;
            body.TextBody = email.PlaintextAlternativeBody;
        }
        else if (email.IsHtml)
        {
            body.HtmlBody = email.Body;
        }
        else
        {
            body.TextBody = email.Body;
        }

        foreach (var attachment in email.Attachments)
            AddAttachment(body, attachment);

        message.Body = body.ToMessageBody();
        return message;
    }

    private static void AddAddresses(InternetAddressList destination, IEnumerable<Address> addresses)
    {
        foreach (var address in addresses)
            destination.Add(new MailboxAddress(address.Name ?? string.Empty, address.EmailAddress));
    }

    private static void AddAttachment(BodyBuilder body, Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment.Data);
        var contentType = ContentType.Parse(attachment.ContentType ?? "application/octet-stream");
        var copy = CopyAttachment(attachment.Data);

        if (attachment.IsInline)
        {
            var resource = body.LinkedResources.Add(attachment.Filename ?? "attachment", copy, contentType);
            resource.ContentId = attachment.ContentId ?? MimeUtils.GenerateMessageId();
            return;
        }

        var mimePart = body.Attachments.Add(attachment.Filename ?? "attachment", copy, contentType);
        if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            mimePart.ContentId = attachment.ContentId;
    }

    private static MemoryStream CopyAttachment(Stream source)
    {
        var copy = new MemoryStream();
        var originalPosition = source.CanSeek ? source.Position : 0;
        try
        {
            if (source.CanSeek)
                source.Position = 0;
            source.CopyTo(copy);
        }
        finally
        {
            if (source.CanSeek)
                source.Position = originalPosition;
        }
        copy.Position = 0;
        return copy;
    }
}
