using CMS.Core;
using CMS.DataEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Text.Json;
using XperienceCommunity.Sustainability.Models;

namespace XperienceCommunity.Sustainability.Services;

public interface ISustainabilityService
{
    Task<SustainabilityResponse?> GetLastReport(int webPageItemID, string languageName);

    Task<SustainabilityResponse?> RunNewReport(string url, int webPageItemID, string languageName);
}

public class SustainabilityService : ISustainabilityService
{
    private readonly IWebHostEnvironment _env;
    private readonly IEventLogService _eventLogService;
    private readonly IInfoProvider<SustainabilityPageDataInfo> _sustainabilityPageDataInfoProvider;

    public SustainabilityService(IWebHostEnvironment env, IEventLogService eventLogService, IInfoProvider<SustainabilityPageDataInfo> sustainabilityPageDataInfoProvider)
    {
        _env = env;
        _eventLogService = eventLogService;
        _sustainabilityPageDataInfoProvider = sustainabilityPageDataInfoProvider;
    }

    public async Task<SustainabilityResponse?> RunNewReport(string url, int webPageItemID, string languageName)
    {
        var uri = new Uri(url);
        var baseUrl = uri.GetLeftPart(UriPartial.Authority);
        var scriptUrl = $"{baseUrl}/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js";

        _eventLogService.LogInformation(nameof(SustainabilityService), nameof(RunNewReport),
            $"RunNewReport called with requestedUrl: {url}, baseUrl: {baseUrl}, scriptUrl: {scriptUrl}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        var context = await browser.NewContextAsync(new()
        {
            BypassCSP = true
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        try
        {
            await page.EvaluateAsync($@"() => {{  
                import('{scriptUrl}')  
                    .then(m => m.reportEmissions?.())  
                    .catch(e => console.error('reportEmissions failed', e));  
            }}");

            var locator = page.Locator("[data-testid='sustainabilityData']");
            await locator.WaitForAsync(new() { Timeout = 60000, State = WaitForSelectorState.Visible });

            var dataJson = await locator.TextContentAsync();

            if (string.IsNullOrWhiteSpace(dataJson))
            {
                return null;
            }

            var sustainabilityData = JsonSerializer.Deserialize<SustainabilityData>(dataJson);
            if (sustainabilityData?.resources == null)
            {
                return null;
            }

            var resourceGroups = Enum.GetValues<ResourceGroupType>()
                .Select(type => GetExternalResourceGroup(type, sustainabilityData.resources))
                .ToList();

            var sustainabilityResponse = new SustainabilityResponse(DateTime.UtcNow)
            {
                TotalSize = (sustainabilityData.pageWeight ?? 0) / 1024m,
                TotalEmissions = sustainabilityData.emissions?.co2 ?? 0,
                CarbonRating = sustainabilityData.carbonRating,
                ResourceGroups = resourceGroups,
            };

            await LogSustainabilityResponse(sustainabilityResponse, webPageItemID, languageName);

            return sustainabilityResponse;
        }
        catch (TimeoutException ex)
        {
            _eventLogService.LogError(nameof(SustainabilityService), nameof(RunNewReport),
                $"Timeout occurred while waiting for sustainability data: {ex.Message}");
            return null;
        }
        finally
        {
            await browser.CloseAsync();
        }
    }

    public async Task<SustainabilityResponse?> GetLastReport(int webPageItemID, string languageName)
    {
        var sustainabilityPageDataInfos = await _sustainabilityPageDataInfoProvider.Get()
            .TopN(1)
            .WhereEquals(nameof(SustainabilityPageDataInfo.WebPageItemID), webPageItemID)
            .WhereEquals(nameof(SustainabilityPageDataInfo.LanguageName), languageName)
            .OrderByDescending(nameof(SustainabilityPageDataInfo.DateCreated))
            .GetEnumerableTypedResultAsync();

        var sustainabilityPageDataInfo = sustainabilityPageDataInfos.FirstOrDefault();

        if (sustainabilityPageDataInfo is null)
        {
            return null;
        }

        return new SustainabilityResponse(sustainabilityPageDataInfo.DateCreated)
        {
            TotalSize = sustainabilityPageDataInfo.TotalSize,
            TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
            CarbonRating = sustainabilityPageDataInfo.CarbonRating,
            ResourceGroups = JsonSerializer.Deserialize<List<ExternalResourceGroup>>(sustainabilityPageDataInfo.ResourceGroups),
        };
    }

    private async Task LogSustainabilityResponse(SustainabilityResponse sustainabilityResponse, int webPageItemID, string languageName)
    {
        var infoObject = new SustainabilityPageDataInfo()
        {
            WebPageItemID = webPageItemID,
            LanguageName = languageName,
            DateCreated = sustainabilityResponse.DateCreated,
            TotalSize = sustainabilityResponse.TotalSize,
            TotalEmissions = sustainabilityResponse.TotalEmissions,
            CarbonRating = sustainabilityResponse.CarbonRating,
            ResourceGroups = JsonSerializer.Serialize(sustainabilityResponse.ResourceGroups),
        };

        await _sustainabilityPageDataInfoProvider.SetAsync(infoObject);
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
            resourceList.Add(new ExternalResource(resource.name, (resource.transferSize ?? 0) / 1024m));
        }

        return new ExternalResourceGroup(groupType)
        {
            TotalSize = transferSize / 1024m,
            Resources = resourceList
        };
    }
}
