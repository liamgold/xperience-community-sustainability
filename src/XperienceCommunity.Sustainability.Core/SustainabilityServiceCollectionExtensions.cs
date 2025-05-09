using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Sustainability.Core.Services;

namespace XperienceCommunity.Sustainability.Core;

public static class SustainabilityServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for Sustainability functionality
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddXperienceCommunitySustainability(this IServiceCollection services)
    {
        services.AddSingleton<ISustainabilityService, SustainabilityService>();

        return services;
    }
}
