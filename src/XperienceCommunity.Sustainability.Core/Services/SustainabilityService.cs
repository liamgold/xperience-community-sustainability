using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Text.Json;
using XperienceCommunity.Sustainability.Core.Models;

namespace XperienceCommunity.Sustainability.Core.Services;

public interface ISustainabilityService
{
    Task<SustainabilityResponse?> GetSustainabilityData(string url);
}

public class SustainabilityService : ISustainabilityService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SustainabilityService> _logger;

    public SustainabilityService(IWebHostEnvironment env, ILogger<SustainabilityService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<SustainabilityResponse?> GetSustainabilityData(string url)
    {
        var uri = new Uri(url);
        var baseUrl = uri.GetLeftPart(UriPartial.Authority);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        page.Console += (_, msg) =>
        {
            if (_env.IsDevelopment())
            {
                _logger.LogInformation("[Browser Console] {Type}: {Text}", msg.Type, msg.Text);
            }
            else
            {
                if (msg.Type == "error" || msg.Type == "warning")
                {
                    _logger.LogWarning("[Browser Console] {Type}: {Text}", msg.Type, msg.Text);
                }
            }
        };

        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.EvaluateAsync($@"() => {{  
            import('{baseUrl}/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js')  
                .then(m => m.reportEmissions?.())  
                .catch(e => console.error('reportEmissions failed', e));  
        }}");

        var locator = page.Locator("[data-testid='sustainabilityData']");
        await locator.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });

        var dataJson = await locator.TextContentAsync();

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            _logger.LogWarning("No sustainability data found on the page.");
            return null;
        }

        var sustainabilityData = JsonSerializer.Deserialize<SustainabilityData>(dataJson);

        if (sustainabilityData?.resources == null)
        {
            _logger.LogWarning("Sustainability data resources are null.");
            return null;
        }

        var resourceGroups = Enum.GetValues<ResourceGroupType>()
            .Select(type => GetExternalResourceGroup(type, sustainabilityData.resources))
            .ToList();

        await browser.CloseAsync();

        return new SustainabilityResponse
        {
            TotalSize = sustainabilityData.pageWeight ?? 0,
            TotalEmissions = sustainabilityData.emissions?.co2 ?? 0,
            CarbonRating = sustainabilityData.carbonRating,
            ResourceGroups = resourceGroups
        };
    }

    private static ExternalResourceGroup GetExternalResourceGroup(ResourceGroupType groupType, IList<Resource> resources)
    {
        var initiator = ExternalResourceGroup.GetInitiatorType(groupType);
        var resourcesByType = resources.Where(x => !string.IsNullOrEmpty(x.initiatorType) && x.initiatorType.Equals(initiator) && x.transferSize > 0);

        var transferSize = 0;
        var resourceList = new List<ExternalResource>();
        foreach (var resource in resourcesByType.OrderByDescending(x => x.transferSize))
        {
            if (string.IsNullOrEmpty(resource.name))
            {
                continue;
            }

            transferSize += resource.transferSize.GetValueOrDefault();
            resourceList.Add(new ExternalResource(resource.name, resource.transferSize));
        }

        return new ExternalResourceGroup(groupType)
        {
            TotalSize = transferSize,
            Resources = resourceList
        };
    }
}
