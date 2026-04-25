using Aero.Auth.Services;
using Aero.Core.Identity;
using Aero.MartenDB.Extensions;
using Aero.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Aero.Common.Web.Extensions;

public static class SocialLoginExtensions
{
    // todo - merged two files into this one - the following method is basic but may work. compare to simliar methods below
    public static AuthenticationBuilder AddSocialLogins(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider(); 
        var config = sp.GetRequiredService<IConfiguration>();

        var authBuidler = services.AddAuthentication();
        authBuidler
            .AddFacebook(opts =>
            {
                opts.AppId = config["Authentication:Facebook:AppId"]
                             ?? throw new ArgumentNullException(opts.AppId, "facebook appid cannot be null");
                opts.AppSecret = config["Authentication:Facebook:AppSecret"]
                                 ?? throw new ArgumentNullException(opts.AppSecret, "facebook appsecret cannot be null");
                opts.AccessDeniedPath = "/AccessDeniedPathInfo";
            })
            .AddGoogle(opts =>
            {
                var googleAuthNSection =
                    config.GetSection("Authentication:Google");

                opts.ClientId = googleAuthNSection["ClientId"]
                                ?? throw new ArgumentNullException(opts.ClientId, "google clientid cannot be null");
                opts.ClientSecret = googleAuthNSection["ClientSecret"]
                                    ?? throw new ArgumentNullException(opts.ClientSecret, "google clientsecret cannot be null");
            });

        return authBuidler;
    }

    /// <summary>
    /// Adds comprehensive authentication services including ASP.NET Core Identity, 
    /// Passkeys/WebAuthn, OpenIddict, and social authentication providers.
    /// </summary>
    /// <param name="env">The hosting environment</param>
    /// <param name="config">Configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static AuthenticationBuilder AddAeroAuthentication(this IServiceCollection services, IHostEnvironment env,
        IConfiguration config)
    {
        var useAeroDb = config.GetValue<bool>("Identity:UseAeroDB");


        // Configure ASP.NET Core Identity
        var identityBuilder = services.AddIdentity<AeroUser, AeroRole>(opts =>
        {
            opts.Password.RequireDigit = true;
            opts.Password.RequireLowercase = true;
            opts.Password.RequireNonAlphanumeric = true;
            opts.Password.RequireUppercase = true;
            opts.Password.RequiredLength = 8;

            opts.User.RequireUniqueEmail = true;
            opts.SignIn.RequireConfirmedEmail = false; // Set to true if email confirmation is implemented
        });

        //services.AddAeroPersistence(config);
        identityBuilder.AddAerodentityStores<AeroUser, AeroRole>(options =>
        {
            options.AutoSaveChanges = true;
        });


        // services.AddOpenTelemetry()
        //     .WithMetrics(metrics =>
        //     {
        //         metrics.AddWebAuthnNet();
        //         metrics.AddPrometheusExporter();
        //     });
        // services.AddSingleton<IRegistrationCeremonyHandleService, DefaultRegistrationCeremonyHandleService>();
        // services.AddSingleton<IAuthenticationCeremonyHandleService, DefaultAuthenticationCeremonyHandleService>();
        // services.AddSingleton<IUserService, DefaultUserService>();

        // Register Passkey/WebAuthn service implementations
        // services.AddScoped<IUserService, DefaultUserService>();
        // services.AddScoped<IRegistrationCeremonyHandleService, DefaultRegistrationCeremonyHandleService>();
        // services.AddScoped<IAuthenticationCeremonyHandleService, DefaultAuthenticationCeremonyHandleService>();

        // Add Data Protection for cookie encryption
        services.AddDataProtection();

        // Configure cookie settings in Program.cs
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax; // or None if using HTTPS
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".microbians.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".microbians.ExternalAuth";
        });

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
            //.PersistKeysToAzureBlobStorage(connectionString, containerName, blobName)
            //.PersistKeysToRegistry(Registry.CurrentUser)
            .SetApplicationName("microbians");

        var authBuilder = services.AddAuthentication()
            .AddCookie(static options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = false;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = config.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"]
                                ?? throw new InvalidOperationException("JWT SecretKey not configured.");
                ;
                var key = Encoding.ASCII.GetBytes(secretKey);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "localhost",
                    ValidAudience = jwtSettings["Audience"] ?? "localhost",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            }).AddSocialAuthentication(config);


        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        // Configure authentication
        // services.AddAuthentication(options => {
        //         // Default scheme for web pages is Cookies
        //         options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //         options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //         // API requests use JWT
        //         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //     })
        //     .AddCookie(options => {
        //         options.LoginPath = "/auth/login";
        //         options.LogoutPath = "/auth/logout";
        //         options.AccessDeniedPath = "/auth/access-denied";
        //     })
        //     .AddJwtBearer(options => {
        //         var jwtSettings = config.GetSection("JwtSettings");
        //         var secretKey = jwtSettings["SecretKey"] 
        //             ?? throw new InvalidOperationException("JWT SecretKey not configured.");
        //         var key = Encoding.ASCII.GetBytes(secretKey);
        //
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidateAudience = true,
        //             ValidateLifetime = true,
        //             ValidateIssuerSigningKey = true,
        //             ValidIssuer = jwtSettings["Issuer"],
        //             ValidAudience = jwtSettings["Audience"],
        //             IssuerSigningKey = new SymmetricSecurityKey(key)
        //         };
        //     });

        // Add production-grade token services
        // Register persistence based on configuration
        if (useAeroDb)
        {
            services.AddScoped<IJwtSigningKeyPersistence, MartenJwtSigningKeyPersistence>();
        }
        else
        {
            // For now, use a fallback in-memory or config-based implementation
            // This will be replaced with EF Core implementation when created
            services.AddScoped<IJwtSigningKeyPersistence>(provider =>
            {
                // Temporary: Use in-memory JWT key store
                // TODO: Replace with EF Core implementation
                var logger = provider.GetRequiredService<ILogger<JwtSigningKeyStore>>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                return new InMemoryJwtSigningKeyPersistence();
            });
        }

        services.AddScoped<IJwtSigningKeyStore, JwtSigningKeyStore>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IApiKeyFactory, DefaultApiKeyFactory>();
        services.AddScoped<IApiKeyGenerator, HashedApiKeyGenerator>();

        if (useAeroDb)
        {
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        }

        // Add memory cache for token store caching
        services.AddMemoryCache();

        return authBuilder;
    }

    /// <summary>
    /// Adds social authentication providers (Google, Twitter, etc.)
    /// </summary>
    /// <param name="authBuilder">The authentication builder returned after calling .AddAuthentication()</param>
    /// <param name="config">Configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static AuthenticationBuilder AddSocialAuthentication(
        this AuthenticationBuilder authBuilder,
        IConfiguration config)
    {
        // Google OAuth
        var googleClientId = config["Authentication:Google:ClientId"];
        var googleClientSecret = config["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.CallbackPath = "/api/auth/external/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme; // ✅ CRITICAL

                // Configure correlation cookie to prevent state errors
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
            });
        }

        // Twitter OAuth
        var twitterConsumerKey = config["Authentication:Twitter:ConsumerKey"];
        var twitterConsumerSecret = config["Authentication:Twitter:ConsumerSecret"];
        if (!string.IsNullOrEmpty(twitterConsumerKey) && !string.IsNullOrEmpty(twitterConsumerSecret))
        {
            authBuilder.AddTwitter(options =>
            {
                options.ConsumerKey = twitterConsumerKey;
                options.ConsumerSecret = twitterConsumerSecret;
                options.CallbackPath = "/api/auth/external/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme; // ✅ CRITICAL

                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
            });
        }

        // Microsoft OAuth
        var microsoftClientId = config["Authentication:Microsoft:ClientId"];
        var microsoftClientSecret = config["Authentication:Microsoft:ClientSecret"];
        if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftClientId;
                options.ClientSecret = microsoftClientSecret;
                options.CallbackPath = "/api/auth/external/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme; // ✅ CRITICAL

                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
            });
        }

        // Facebook OAuth
        var facebookAppId = config["Authentication:Facebook:AppId"];
        var facebookAppSecret = config["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
        {
            authBuilder.AddFacebook(options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
                options.CallbackPath = "/api/auth/external/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme; // ✅ CRITICAL

                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
            });
        }

        return authBuilder;
    }
}