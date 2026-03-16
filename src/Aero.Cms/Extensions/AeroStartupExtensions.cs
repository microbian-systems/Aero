

using Microsoft.Extensions.DependencyInjection;
using Aero.Cms;
using Aero.Cms.Services;

public static class AeroStartupExtensions
{
    public static IServiceCollection AddAero(this IServiceCollection services,
        Action<AeroServiceBuilder> options = null, ServiceLifetime scope = ServiceLifetime.Scoped)
    {
        var serviceBuilder = new AeroServiceBuilder(services);

        options?.Invoke(serviceBuilder);

        services.AddSingleton<IContentFactory, ContentFactory>();

        services.AddScoped<IApi, Api>();
        services.AddScoped<Config>();

        // Register core services
        services.AddScoped<IAliasService>(sp => sp.GetRequiredService<IApi>().Aliases);
        services.AddScoped<IArchiveService>(sp => sp.GetRequiredService<IApi>().Archives);
        services.AddScoped<IContentGroupService>(sp => sp.GetRequiredService<IApi>().ContentGroups);
        services.AddScoped<IContentService>(sp => sp.GetRequiredService<IApi>().Content);
        services.AddScoped<IContentTypeService>(sp => sp.GetRequiredService<IApi>().ContentTypes);
        services.AddScoped<ILanguageService>(sp => sp.GetRequiredService<IApi>().Languages);
        services.AddScoped<IMediaService>(sp => sp.GetRequiredService<IApi>().Media);
        services.AddScoped<IPageService>(sp => sp.GetRequiredService<IApi>().Pages);
        services.AddScoped<IPageTypeService>(sp => sp.GetRequiredService<IApi>().PageTypes);
        services.AddScoped<IParamService>(sp => sp.GetRequiredService<IApi>().Params);
        services.AddScoped<IPostService>(sp => sp.GetRequiredService<IApi>().Posts);
        services.AddScoped<IPostTypeService>(sp => sp.GetRequiredService<IApi>().PostTypes);
        services.AddScoped<ISiteService>(sp => sp.GetRequiredService<IApi>().Sites);
        services.AddScoped<ISiteTypeService>(sp => sp.GetRequiredService<IApi>().SiteTypes);

        return services;
    }
}