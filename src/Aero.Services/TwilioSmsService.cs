using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Aero.Services;

public class TwilioSmsService(AppSettings settings, ILogger<TwilioSmsService> log) : ISmsService
{
    private readonly AppSettings settings = settings;
    private readonly string accountSid = settings.Twilio.AccountSid;
    private readonly string authToken = settings.Twilio.AuthToken;

    public async Task SendSms(string from, string to, string body)
    {
        log.LogInformation($"sending twilio sms to {to} with {body}");
        TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            body: body,
            from: new Twilio.Types.PhoneNumber(from),
            to: new Twilio.Types.PhoneNumber(to)
        );
            
        log.LogInformation($"message: {message}");
    }
}