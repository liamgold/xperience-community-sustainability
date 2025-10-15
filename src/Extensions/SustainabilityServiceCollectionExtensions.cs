using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Sustainability.Admin;
using XperienceCommunity.Sustainability.Models;
using XperienceCommunity.Sustainability.Services;

namespace XperienceCommunity.Sustainability;

public static class SustainabilityServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for Sustainability functionality
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddXperienceCommunitySustainability(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SustainabilityOptions>(configuration.GetSection("Sustainability"));

        services.AddScoped<ISustainabilityService, SustainabilityService>();
        services.AddSingleton<ISustainabilityModuleInstaller, SustainabilityModuleInstaller>();

        return services;
    }
}
