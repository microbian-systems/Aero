namespace Aero.Core;

/// <summary>
/// Represents a record for AppSettings.
/// </summary>
public record AppSettings
{
        /// <summary>
    /// Gets or sets the App Name.
    /// </summary>
public string AppName { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the App Version.
    /// </summary>
public string AppVersion { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Setup Complete.
    /// </summary>
public bool SetupComplete { get; init; }
        /// <summary>
    /// Gets or sets the Client Urls.
    /// </summary>
public string[] ClientUrls { get; init; } = [];
        /// <summary>
    /// Gets or sets the Domain Name.
    /// </summary>
public string DomainName { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Organization Name.
    /// </summary>
public string OrganizationName { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Admin Email.
    /// </summary>
public string AdminEmail { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Reply To Email.
    /// </summary>
public string ReplyToEmail { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Use Proxy.
    /// </summary>
public bool UseProxy { get; init; }
        /// <summary>
    /// Gets or sets the Cloud Flare Only Connections.
    /// </summary>
public bool CloudFlareOnlyConnections { get; init; }
        /// <summary>
    /// Gets or sets the Identity Options.
    /// </summary>
public AeroIdentityOptions IdentityOptions { get; init; } = new();
        /// <summary>
    /// Gets or sets the Elasticsearch Urls.
    /// </summary>
public List<string> ElasticsearchUrls { get; init; } = [];
        /// <summary>
    /// Gets or sets the App Insights Key.
    /// </summary>
public string AppInsightsKey { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Use Azure Key Vault.
    /// </summary>
public bool UseAzureKeyVault { get; init; }
        /// <summary>
    /// Gets or sets the Key Vault End Point.
    /// </summary>
public string KeyVaultEndPoint { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Enable Hangfire.
    /// </summary>
public bool EnableHangfire { get; init; }
        /// <summary>
    /// Gets or sets the Azure Storage.
    /// </summary>
public AzureStorageEntry AzureStorage { get; init; } = new();
        /// <summary>
    /// Gets or sets the Secret.
    /// </summary>
public string Secret { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Aes Encryption Settings.
    /// </summary>
public AesEncryptionSettings AesEncryptionSettings { get; init; } = new();
        /// <summary>
    /// Gets or sets the Send Grid.
    /// </summary>
public SendGridSettings SendGrid { get; init; } = new();
        /// <summary>
    /// Gets or sets the Twilio.
    /// </summary>
public TwilioSettings Twilio { get; init; } = new();
        /// <summary>
    /// Gets or sets the Stripe.
    /// </summary>
public StripeSettings Stripe { get; init; } = new();
        /// <summary>
    /// Gets or sets the Zip Api.
    /// </summary>
public ZipApiSettings ZipApi { get; init; } = new();
        /// <summary>
    /// Gets or sets the Use Azure Storage.
    /// </summary>
public bool UseAzureStorage { get; init; }
        /// <summary>
    /// Gets or sets the Use Blob Storage.
    /// </summary>
public bool UseBlobStorage { get; init; }
        /// <summary>
    /// Gets or sets the Enable Mini Profiler.
    /// </summary>
public bool EnableMiniProfiler { get; init; }
        /// <summary>
    /// Gets or sets the Valid Issuers.
    /// </summary>
public List<string> ValidIssuers { get; protected init; } = [];
        /// <summary>
    /// Gets or sets the Enable Static File Caching.
    /// </summary>
public bool EnableStaticFileCaching { get; init; }
}

/// <summary>
/// Represents a record for AesEncryptionSettings.
/// </summary>
public record AesEncryptionSettings
{
        /// <summary>
    /// Gets or sets the Key.
    /// </summary>
public string Key { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the IV.
    /// </summary>
public string IV { get; set; } = string.Empty;
}

/// <summary>
/// Represents a record for AzureStorageEntry.
/// </summary>
public record AzureStorageEntry
{
        /// <summary>
    /// Gets or sets the Container Name.
    /// </summary>
public string ContainerName { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Storage Key.
    /// </summary>
public string StorageKey { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Storage Name.
    /// </summary>
public string StorageName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a class for AeroIdentityOptions.
/// </summary>
public class AeroIdentityOptions : BaseOptions
{
        /// <summary>
    /// Initializes a new instance of the <see cref="AeroIdentityOptions"/> class.
    /// </summary>
public AeroIdentityOptions()
    {
        SectionName = nameof(AeroIdentityOptions);
    }

        /// <summary>
    /// Gets or sets the Require Confirmed Account.
    /// </summary>
public bool RequireConfirmedAccount { get; set; }
        /// <summary>
    /// Gets or sets the Require Digit.
    /// </summary>
public bool RequireDigit { get; set; }
        /// <summary>
    /// Gets or sets the Require Lowercase.
    /// </summary>
public bool RequireLowercase { get; set; }
        /// <summary>
    /// Gets or sets the Require Non Alphanumeric.
    /// </summary>
public bool RequireNonAlphanumeric { get; set; }
        /// <summary>
    /// Gets or sets the Require Uppercase.
    /// </summary>
public bool RequireUppercase { get; set; }
        /// <summary>
    /// Gets or sets the Required Length.
    /// </summary>
public int RequiredLength { get; set; }
        /// <summary>
    /// Gets or sets the Required Unique Chars.
    /// </summary>
public int RequiredUniqueChars { get; set; }
        /// <summary>
    /// Gets or sets the Require Unique Email.
    /// </summary>
public bool RequireUniqueEmail { get; set; }
        /// <summary>
    /// Gets or sets the Default Lockout Time Span.
    /// </summary>
public int DefaultLockoutTimeSpan { get; set; }
        /// <summary>
    /// Gets or sets the Max Failed Access Attempts.
    /// </summary>
public int MaxFailedAccessAttempts { get; set; }
        /// <summary>
    /// Gets or sets the Lockout Allowed For New Users.
    /// </summary>
public bool LockoutAllowedForNewUsers { get; set; }

        /// <summary>
    /// Gets or sets the Allowed User Name Characters.
    /// </summary>
public string AllowedUserNameCharacters { get; protected set; } =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
}

/// <summary>
/// Represents a record for SendGridSettings.
/// </summary>
public record SendGridSettings
{
        /// <summary>
    /// Gets or sets the Key.
    /// </summary>
[JsonPropertyName("key")]
    public string Key { get; set; }

        /// <summary>
    /// Gets or sets the From.
    /// </summary>
[JsonPropertyName("from")]
    public string From { get; set; }

        /// <summary>
    /// Gets or sets the From Name.
    /// </summary>
[JsonPropertyName("from_name")]
    public string FromName { get; set; }
}

/// <summary>
/// Represents a record for StripeSettings.
/// </summary>
public record StripeSettings
{
        /// <summary>
    /// Gets or sets the Secret Key.
    /// </summary>
[JsonPropertyName("secret_key")]
    public string SecretKey { get; set; }
}

/// <summary>
/// Represents a record for TwilioSettings.
/// </summary>
public record TwilioSettings
{
        /// <summary>
    /// Gets or sets the Account Sid.
    /// </summary>
[JsonPropertyName("account_sID")]
    public string AccountSid { get; set; }

        /// <summary>
    /// Gets or sets the Auth Token.
    /// </summary>
[JsonPropertyName("auth_token")]
    public string AuthToken { get; set; }
}

/// <summary>
/// Represents a record for ZipApiSettings.
/// </summary>
public record ZipApiSettings
{
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[JsonPropertyName("username")]
    public string Username { get; set; }

        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[JsonPropertyName("Password")]
    public string Password { get; set; }

        /// <summary>
    /// Gets or sets the Url.
    /// </summary>
[JsonPropertyName("url")]
    public string Url { get; set; }

        /// <summary>
    /// Gets or sets the Api Key.
    /// </summary>
[JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; }

        /// <summary>
    /// Gets or sets the Js Api Key.
    /// </summary>
[JsonPropertyName("JsApiKey")]
    public string JsApiKey { get; set; }
}
