using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail.Razor;
using Aero.Services.Mail;
using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net;
using TUnit.Assertions;
using TUnit.Core;

namespace Aero.Services.Mail.Tests;

[Timeout(20_000)]
public sealed class MailKitDependencyAndCompatibilityTests
{
    [Test]
    public async Task Services_dependency_graph_uses_the_centrally_approved_mailkit_and_mimekit_versions(
        CancellationToken cancellationToken)
    {
        using var dependencies = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Aero.Services.Mail.Tests.deps.json"), cancellationToken));

        var libraries = dependencies.RootElement.GetProperty("libraries").EnumerateObject()
            .Select(library => library.Name).ToArray();

        await Assert.That(libraries).Contains("MailKit/4.17.0");
        await Assert.That(libraries).Contains("MimeKit/4.17.0");
        await Assert.That(libraries.Any(library => library.StartsWith("MailKit/2.", StringComparison.Ordinal))).IsFalse();
        await Assert.That(libraries.Any(library => library.StartsWith("MimeKit/2.", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task Services_dependency_graph_uses_the_aligned_razorlight_and_roslyn_family(
        CancellationToken cancellationToken)
    {
        using var dependencies = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Aero.Services.Mail.Tests.deps.json"), cancellationToken));

        var libraries = dependencies.RootElement.GetProperty("libraries").EnumerateObject()
            .Select(library => library.Name).ToArray();

        await Assert.That(libraries).Contains("RazorLight/2.3.1");
        await Assert.That(libraries).Contains("Microsoft.CodeAnalysis.CSharp/5.0.0");
        await Assert.That(libraries).Contains("Microsoft.CodeAnalysis.Common/5.0.0");
        await Assert.That(libraries.Any(library => library.StartsWith("RazorLight/2.0.0-rc.3", StringComparison.Ordinal))).IsFalse();
        await Assert.That(libraries.Any(library => library.StartsWith("Microsoft.CodeAnalysis.CSharp/3.7.0", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task Razor_renderer_preserves_typed_model_loop_html_encoding_and_repeated_rendering(
        CancellationToken cancellationToken)
    {
        var renderer = new RazorRenderer(typeof(MailKitDependencyAndCompatibilityTests));
        const string template = "@model Aero.Services.Mail.Tests.ProposalModel\n<ul>@foreach (var item in Model.Items) { <li>@item</li> }</ul>";
        var model = new ProposalModel(["Modular terminal", "Safety & compliance"]);

        var first = await renderer.ParseAsync(template, model, true).WaitAsync(cancellationToken);
        var second = await renderer.ParseAsync(template, model, true).WaitAsync(cancellationToken);

        await Assert.That(first).Contains("<li>Modular terminal</li>");
        await Assert.That(first).Contains("Safety &amp; compliance");
        await Assert.That(second).IsEqualTo(first);
    }

    [Test]
    public async Task MailKit_sender_preserves_addresses_subject_html_body_and_attachment_over_local_smtp(
        CancellationToken cancellationToken)
    {
        await using var smtpServer = await SyntheticSmtpServer.StartAsync(cancellationToken);
        var sender = new MailKitSmtpSender(new MailKitSmtpSenderOptions
        {
            Host = "127.0.0.1",
            Port = smtpServer.Port,
            TransportSecurity = MailKitSmtpTransportSecurity.Plaintext,
            AllowInsecureTransport = true
        });

        await using var attachment = new MemoryStream(Encoding.UTF8.GetBytes("proposal attachment"));
        var email = new Email(null, sender)
            .SetFrom("sender@example.test", "Contractor AI")
            .To("recipient@example.test", "Contracting Officer")
            .CC("reviewer@example.test", "Proposal reviewer")
            .BCC("archive@example.test")
            .ReplyTo("replies@example.test", "Proposal desk")
            .Subject("Proposal for modular terminals – résumé")
            .Body("<strong>Prepared proposal</strong>", true)
            .Header("X-Contract-Notice", "abc-123")
            .Attach(new Attachment
            {
                Filename = "proposál.txt",
                ContentType = "text/plain",
                Data = attachment
            });

        var response = await email.SendAsync(cancellationToken);
        var message = await smtpServer.WaitForMessageAsync(cancellationToken);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(message).Contains("From: Contractor AI <sender@example.test>");
        await Assert.That(message).Contains("To: Contracting Officer <recipient@example.test>");
        await Assert.That(message).Contains("Cc: Proposal reviewer <reviewer@example.test>");
        await Assert.That(message).Contains("Reply-To: Proposal desk <replies@example.test>");
        await Assert.That(message).Contains("X-Contract-Notice: abc-123");
        await Assert.That(message).Contains("<strong>Prepared proposal</strong>");
        await Assert.That(message).Contains("filename*=");
        await Assert.That(message).Contains("propos%E1l.txt");
        await Assert.That(message).Contains("Content-Transfer-Encoding: 7bit");
        await Assert.That(message).Contains("proposal attachment");
        await Assert.That(smtpServer.Commands).Contains("RCPT TO:<archive@example.test>");
    }

    [Test]
    public async Task MailKit_sender_does_not_take_ownership_of_attachment_streams(
        CancellationToken cancellationToken)
    {
        await using var smtpServer = await SyntheticSmtpServer.StartAsync(cancellationToken);
        var sender = new MailKitSmtpSender(new MailKitSmtpSenderOptions
        {
            Host = "127.0.0.1",
            Port = smtpServer.Port,
            TransportSecurity = MailKitSmtpTransportSecurity.Plaintext,
            AllowInsecureTransport = true
        });
        await using var attachment = new MemoryStream(Encoding.UTF8.GetBytes("source remains open"));
        attachment.Position = 4;
        var email = new Email(null, sender).SetFrom("sender@example.test").To("recipient@example.test")
            .Subject("Proposal").Body("Body").Attach(new Attachment { Filename = "source.txt", Data = attachment });

        var response = await email.SendAsync(cancellationToken);
        await smtpServer.WaitForMessageAsync(cancellationToken);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(attachment.CanRead).IsTrue();
        await Assert.That(attachment.Position).IsEqualTo(4L);
    }

    [Test]
    public async Task MailKit_sender_registration_replaces_the_default_sender_without_changing_fluentemail_contracts(
        CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddFluentEmail("sender@example.test")
            .AddMailKitSmtpSender(new MailKitSmtpSenderOptions
            {
                Host = "smtp.example.test",
                Port = 2525,
                TransportSecurity = MailKitSmtpTransportSecurity.TlsOnConnect,
                Username = "smtp-user",
                Password = "smtp-password"
            });

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.That(sender).IsTypeOf<MailKitSmtpSender>();
        cancellationToken.ThrowIfCancellationRequested();
    }

    [Test]
    public async Task MailKit_sender_rejects_credentials_over_plaintext_transport(CancellationToken cancellationToken)
    {
        await Assert.That(() => new MailKitSmtpSender(new MailKitSmtpSenderOptions
        {
            Host = "smtp.example.test",
            Port = 2525,
            TransportSecurity = MailKitSmtpTransportSecurity.Plaintext,
            AllowInsecureTransport = true,
            Username = "smtp-user",
            Password = "smtp-password"
        })).Throws<FluentValidation.ValidationException>();
        cancellationToken.ThrowIfCancellationRequested();
    }

    [Test]
    public async Task MailKit_sender_returns_legacy_cancellation_response_before_dispatch(CancellationToken cancellationToken)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var sender = new MailKitSmtpSender(new MailKitSmtpSenderOptions
        {
            Host = "127.0.0.1",
            Port = 1,
            TransportSecurity = MailKitSmtpTransportSecurity.Plaintext,
            AllowInsecureTransport = true
        });
        var email = new Email(null, sender).SetFrom("sender@example.test").To("recipient@example.test").Subject("Cancelled").Body("Cancelled");

        var response = await email.SendAsync(cancellation.Token);

        await Assert.That(response.Successful).IsFalse();
        await Assert.That(response.ErrorMessages).Contains("Message was cancelled by cancellation token.");
        cancellationToken.ThrowIfCancellationRequested();
    }

    [Test]
    public async Task MailKit_sender_reports_smtp_rejection_without_exposing_transport_details(CancellationToken cancellationToken)
    {
        var transport = new RecordingTransport { SendFailure = new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.TransactionFailed, "recipient@example.test rejected") };
        var sender = CreateSender(transport);
        var response = await new Email(null, sender).SetFrom("sender@example.test").To("recipient@example.test").Subject("Subject").Body("Body").SendAsync(cancellationToken);
        await Assert.That(response.Successful).IsFalse();
        await Assert.That(response.ErrorMessages).Contains("SMTP server rejected the message.");
        await Assert.That(response.ErrorMessages.Any(message => message.Contains("recipient@example.test", StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task MailKit_sender_uses_native_sync_transport_path(CancellationToken cancellationToken)
    {
        var transport = new RecordingTransport();
        var sender = CreateSender(transport);
        var response = new Email(null, sender).SetFrom("sender@example.test").To("recipient@example.test").Subject("Subject").Body("Body").Send(cancellationToken);
        await Assert.That(response.Successful).IsTrue();
        await Assert.That(transport.SyncSendCalled).IsTrue();
        await Assert.That(transport.AsyncSendCalled).IsFalse();
    }

    [Test]
    public async Task MailKit_sender_bounds_post_acceptance_disconnect_with_the_operation_token(CancellationToken cancellationToken)
    {
        var transport = new RecordingTransport();
        var sender = CreateSender(transport);
        var response = await new Email(null, sender).SetFrom("sender@example.test").To("recipient@example.test").Subject("Subject").Body("Body").SendAsync(cancellationToken);

        await Assert.That(response.Successful).IsTrue();
        await Assert.That(transport.DisconnectToken.CanBeCanceled).IsTrue();
    }

    private static MailKitSmtpSender CreateSender(RecordingTransport transport) => new(
        new MailKitSmtpSenderOptions { Host = "smtp.example.test", Port = 2525, TransportSecurity = MailKitSmtpTransportSecurity.Plaintext, AllowInsecureTransport = true },
        new RecordingFactory(transport));

    private sealed class RecordingFactory(RecordingTransport transport) : IMailKitSmtpClientFactory { public IMailKitSmtpClient Create() => transport; }

    private sealed class RecordingTransport : IMailKitSmtpClient
    {
        public Exception? SendFailure { get; init; }
        public bool SyncSendCalled { get; private set; }
        public bool AsyncSendCalled { get; private set; }
        public CancellationToken DisconnectToken { get; private set; }
        public void Connect(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken) { }
        public Task ConnectAsync(string host, int port, SecureSocketOptions security, CancellationToken cancellationToken) => Task.CompletedTask;
        public void Authenticate(ICredentials credentials, CancellationToken cancellationToken) { }
        public Task AuthenticateAsync(ICredentials credentials, CancellationToken cancellationToken) => Task.CompletedTask;
        public string Send(MimeMessage message, CancellationToken cancellationToken) { SyncSendCalled = true; if (SendFailure is not null) throw SendFailure; return "sync-id"; }
        public Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken) { AsyncSendCalled = true; if (SendFailure is not null) throw SendFailure; return Task.FromResult("async-id"); }
        public void Disconnect(bool quit, CancellationToken cancellationToken) => DisconnectToken = cancellationToken;
        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken) { DisconnectToken = cancellationToken; return Task.CompletedTask; }
        public void Dispose() { }
    }

    private sealed class SyntheticSmtpServer : IAsyncDisposable
    {
        private readonly TcpListener listener;
        private readonly CancellationTokenSource lifetime = new();
        private readonly TaskCompletionSource<string> message = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Task serverTask;
        private readonly List<string> commands = [];
        private readonly string? expectedUsername;
        private readonly string? expectedPassword;

        private SyntheticSmtpServer(TcpListener listener, string? expectedUsername, string? expectedPassword)
        {
            this.listener = listener;
            this.expectedUsername = expectedUsername;
            this.expectedPassword = expectedPassword;
            serverTask = AcceptAndCaptureAsync();
        }

        public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

        public bool Authenticated { get; private set; }

        public static Task<SyntheticSmtpServer> StartAsync(CancellationToken cancellationToken) =>
            StartAsync(null, null, cancellationToken);

        public static Task<SyntheticSmtpServer> StartAsync(string? username, string? password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return Task.FromResult(new SyntheticSmtpServer(listener, username, password));
        }

        public Task<string> WaitForMessageAsync(CancellationToken cancellationToken) =>
            message.Task.WaitAsync(cancellationToken);

        public IReadOnlyList<string> Commands => commands;

        private async Task AcceptAndCaptureAsync()
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync(lifetime.Token);
                using var network = client.GetStream();
                using var reader = new StreamReader(network, Encoding.ASCII, false, 1024, true);
                using var writer = new StreamWriter(network, new UTF8Encoding(false), 1024, true) { NewLine = "\r\n", AutoFlush = true };
                await writer.WriteLineAsync("220 local SMTP ready");

                while (await reader.ReadLineAsync(lifetime.Token) is { } command)
                {
                    commands.Add(command);
                    if (command.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase) || command.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250-localhost");
                        if (expectedUsername is not null)
                            await writer.WriteLineAsync("250-AUTH PLAIN");
                        await writer.WriteLineAsync("250 SIZE 10485760");
                    }
                    else if (command.StartsWith("AUTH PLAIN ", StringComparison.OrdinalIgnoreCase))
                    {
                        var payload = Encoding.UTF8.GetString(Convert.FromBase64String(command[11..]));
                        Authenticated = payload == $"\0{expectedUsername}\0{expectedPassword}";
                        await writer.WriteLineAsync(Authenticated ? "235 authenticated" : "535 invalid credentials");
                    }
                    else if (command.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase) || command.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250 OK");
                    }
                    else if (command.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                        var lines = new List<string>();
                        while (await reader.ReadLineAsync(lifetime.Token) is { } line && line != ".")
                            lines.Add(line);
                        message.TrySetResult(string.Join("\r\n", lines));
                        await writer.WriteLineAsync("250 queued");
                    }
                    else if (command.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("221 goodbye");
                        return;
                    }
                    else
                    {
                        await writer.WriteLineAsync("250 OK");
                    }
                }

                message.TrySetException(new InvalidOperationException("The SMTP client disconnected before transmitting a message."));
            }
            catch (Exception exception)
            {
                message.TrySetException(exception);
            }
        }

        public async ValueTask DisposeAsync()
        {
            lifetime.Cancel();
            listener.Stop();
            try { await serverTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (SocketException) { }
            catch (OperationCanceledException) { }
            finally { lifetime.Dispose(); }
        }
    }
}

public sealed record ProposalModel(IReadOnlyList<string> Items);
