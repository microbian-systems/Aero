namespace Aero.Common.Web.Extensions;


// todo - consolidate the default api extensions w/ the Aero extensions
public static class DefaultApiExtensions
{
    public static WebApplicationBuilder ConfigureDefaultApi(this WebApplicationBuilder builder)
    {
        builder.AddDefaultLogging();
        builder.RemoveHeaders();
        builder.AddDefaultApiServices();
        builder.Services.AddHealthChecks();
        //builder.AddApiKeyGenerator();
        builder.AddJwtAuthorization();
        //builder.AddApiAuthDbContext();

        return builder;
    }


    /// <inheritdoc cref="UseDefaultApi(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Action{Microsoft.AspNetCore.Builder.WebApplication},bool)"/>
    /// <param name="builder"></param>
    /// <param name="configure">Additional configuration capabilities for the Default API configuration. Can be overridden</param>
    /// <param name="overrideDefaults"></param>
    public static WebApplication UseDefaultApi(
        this WebApplicationBuilder builder,
        Action<WebApplication> configure,
        bool overrideDefaults = false)
    {
        var app = overrideDefaults switch
        {
            true => builder.Build(),
            false => builder.UseDefaultApi()
        };

        configure.Invoke(app);

        return app;
    }

    /// <summary>
    /// Used to register any Use() methods that require async operations (awaited)
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">An async caapable lambda to configure the web api to use additional features outside of the defaults</param>
    /// <param name="overrideDefaults">used to skip the default api settings (roll your own)</param>
    /// <returns></returns>
    public static WebApplication UseDefaultApi(
        this WebApplicationBuilder builder,
        Func<WebApplication, Task> configure,
        bool overrideDefaults = false)
    {
        var app = builder.UseDefaultApi(app =>
        {
            configure.Invoke(app);
        }, overrideDefaults);

        return app;
    }

    public static WebApplication UseDefaultApi(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.MapControllers();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHealthChecks("/ping");

        return app;
    }

    public static WebApplicationBuilder ConfigureDefaultApi(
        this WebApplicationBuilder builder,
        Action configure,
        bool overrideDefaults = false)
    {
        configure.Invoke();

        if (!overrideDefaults)
            builder.ConfigureDefaultApi();

        return builder;
    }

    public static WebApplicationBuilder ConfigureDefaultApi(
        this WebApplicationBuilder builder,
        Action<WebApplicationBuilder> configure,
        bool overrideDefaults = false)
    {
        configure.Invoke(builder);

        if (!overrideDefaults)
            builder.ConfigureDefaultApi();

        return builder;
    }
}