using Aero.Common.Web.Exceptions;
using Aero.Common.Web.Middleware;
using Aero.Services;
using Aero.Services.Geo;
using Aero.Services.Mail;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aero.Common.Web.Extensions;

public static class AeroWebExtensions
{
    public static WebApplicationBuilder AddAeroDefaultServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAeroDefaultServices(builder.Configuration, builder.Environment);
        return builder;
    }

    public static IServiceCollection AddAeroDefaultServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment host, string connString = "")
    {
        if (string.IsNullOrEmpty(connString))
            connString = config.GetConnectionString("DefaultConnection")
                         ?? throw new ArgumentNullException(nameof(connString), "Connection string is required for Aero services");
        //services.AddAeroIdentity<AeroUser, AeroRole>();
        services.AddAeroCoreServices(config, host);
        //services.AddDataLayerPersistence(config, host);
        //services.AddAeroIdentityDefaults<TUser, AeroIdentityContext>(connString);

        return services;
    }

    public static IServiceCollection AddAeroCoreServices(
        this IServiceCollection services,
        IConfiguration config,
        IWebHostEnvironment host,
        bool enableAntiForgeryProtection = false)
    {
        services.AddEncryptionServices();
        services.AddMapster();
        // if (enableAntiForgeryProtection)
        //     services.ConfigureAntiForgeryOptions();
        //services.AddRequestResponseLogging();
        // if (!host.IsProduction())
        //     services.AddMiniProfilerEx();

        // https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/7.0/default-authentication-scheme
        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-8.0
        services.AddAuthentication(o =>
        {
            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            // Configure cookie authentication options
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            // Configure JWT Bearer options  
            var jwtOptions = config.GetSection("Jwt");
            if (jwtOptions.Exists())
            {
                options.Authority = jwtOptions["Authority"];
                options.Audience = jwtOptions["Audience"];
            }
        });

        // todo - should authorization be initialized here - may be better in cms
        services.AddAuthorization(o =>
        {
            string[] schemes = [

                CookieAuthenticationDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme
            ];
            o.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(schemes)
                .RequireAuthenticatedUser()
                .Build();
        });
        // todo - analyzle if we still need any of the middleware here in this library - may move to cms project

        //services.AddAntiforgery();
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenValidationService, AeroJwtValidationService>();
        services.AddEmailServies(config, host);
        services.ConfigureAppSettings(config, host);
        services.AddAeroCaching(config);
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddTransient<IEmailSender, SendGridMailer>();
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddScoped<IZipApiService, ZipApiService>();
        services.AddScoped<IAeroUserProfileService, AeroUserProfileService>();
        services.AddScoped(typeof(IUserProfileService<>), typeof(AeroUserProfileService<>));

        return services;
    }

    public static IApplicationBuilder UseDefaultAeroServices(this IApplicationBuilder app)
    {
        app.UseAeroMiddleware();
        return app;
    }

    public static IApplicationBuilder UseAeroMiddleware(this IApplicationBuilder app)
    {
        // todo - analyzle if we still need any of the middleware here in this library - may move to cms project
        app.ConfigureExceptionMiddleware();
        //app.UseDefaultLogging();
        app.UseRequestCultureMiddleware();
        //app.UsePerfLogging();
        // app.UseSerilogRequestLogging();
        // app.UseRequestResponseLogging();
        // app.UseCustom404Handler();
        // app.UseCustom401Handler();
        // app.UseCustom400Handler();
        //app.UseRequestResponseLogging();
        // todo - fix CORS/OWasp and Xss later
        //app.UseXssMiddleware();
        // https://github.com/GaProgMan/OwaspHeaders.Core
        // app.UseSecureHeadersMiddleware();


        return app;
    }
}