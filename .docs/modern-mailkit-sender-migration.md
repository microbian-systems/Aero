# Direct MailKit FluentEmail sender migration

`FluentEmail.MailKit` 3.0.2 was removed because it is binary-bound to MailKit 2.10.1. Applications using its `AddMailKitSender` registration must migrate to Aero's `AddMailKitSmtpSender` API and construct `MailKitSmtpSenderOptions`.

The replacement defaults to required STARTTLS. `TlsOnConnect` is explicit. Plaintext SMTP is available only with `AllowInsecureTransport = true` and never accepts credentials. The public factory interface is a testable transport boundary; the default adapter preserves MailKit certificate and revocation validation.

## Registration compatibility

This is a source/binary compatibility break for consumers of the old
`FluentEmail.MailKitSmtp` integration types and `AddMailKitSender` extension.
FluentEmail's `ISender` / `IFluentEmail` calling contract remains unchanged.
Use `Aero.Services.Mail` and `AddMailKitSmtpSender`. The overload accepting
`Func<IServiceProvider, MailKitSmtpSenderOptions>` resolves options per scope;
the direct-options overload delegates to it. No existing Aero.Web SMTP or
Mailtrap registration is silently switched.

```csharp
services.AddFluentEmail("sender@example.test")
    .AddMailKitSmtpSender(sp => new MailKitSmtpSenderOptions
    {
        Host = "smtp.example.test",
        Port = 587,
        TransportSecurity = MailKitSmtpTransportSecurity.StartTlsRequired
    });
```

Credentials, when required, must be supplied by the application's protected
configuration/secret provider, never committed or included in transport errors.
This library change does not activate an AeroVault binding or real SMTP service.
Pickup-directory delivery and the legacy preferred-encoding setting are not
supported by this bounded replacement; consumers using them need a separately
selected sender. Razor rendering remains separate and unchanged by registration.

## Delivery and ownership contract

Native synchronous and asynchronous paths are supported. `OperationTimeout`
bounds the complete attempt, including connect/authenticate/send/cleanup, rather
than resetting for every operation. Pre-dispatch caller cancellation returns the
legacy unsuccessful cancellation response. Confirmed SMTP rejection has a stable
redacted failure; interruption after dispatch begins is reported as an unknown
delivery outcome. Do not automatically retry an unknown outcome. Once SMTP
accepts a send, failed or cancelled QUIT cleanup cannot turn it into failure.

MIME messages and transport clients are disposed. Caller-owned attachment streams
remain open, and seekable stream positions are preserved. Nonempty plaintext
alternatives, including whitespace-only values, retain the legacy text/HTML
mapping independently of `IsHtml`.

The final synthetic focused suite passes 19/19. Mocked TLS-mode/authentication
checks and local loopback delivery are not live TLS-server or production-mail
acceptance. The wider application restore/build gate remains separate.
