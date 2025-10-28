using CMS.Core;
using CMS.DataEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Reflection;
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
    private readonly SustainabilityOptions _options;

    private static readonly string ScriptPath = GetScriptPath();
    private const string SustainabilityDataTestId = "sustainabilityData";

    private static string GetScriptPath()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        return $"/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js?v={version}";
    }

    public SustainabilityService(
        IWebHostEnvironment env,
        IEventLogService eventLogService,
        IInfoProvider<SustainabilityPageDataInfo> sustainabilityPageDataInfoProvider,
        IOptions<SustainabilityOptions> options)
    {
        _env = env;
        _eventLogService = eventLogService;
        _sustainabilityPageDataInfoProvider = sustainabilityPageDataInfoProvider;
        _options = options.Value;
    }

    public async Task<SustainabilityResponse?> RunNewReport(string url, int webPageItemID, string languageName)
    {
        var uri = new Uri(url);
        var baseUrl = uri.GetLeftPart(UriPartial.Authority);
        var scriptUrl = $"{baseUrl}{ScriptPath}";

        _eventLogService.LogInformation(nameof(SustainabilityService), nameof(RunNewReport),
            $"RunNewReport called with requestedUrl: {url}, baseUrl: {baseUrl}, scriptUrl: {scriptUrl}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        await using var context = await browser.NewContextAsync(new()
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

            var locator = page.Locator($"[data-testid='{SustainabilityDataTestId}']");
            await locator.WaitForAsync(new() { Timeout = _options.TimeoutMilliseconds, State = WaitForSelectorState.Visible });

            var dataJson = await locator.TextContentAsync();

            if (string.IsNullOrWhiteSpace(dataJson))
            {
                return null;
            }

            var sustainabilityData = JsonSerializer.Deserialize<SustainabilityData>(dataJson);
            if (sustainabilityData?.Resources == null)
            {
                return null;
            }

            var resourceGroups = Enum.GetValues<ResourceGroupType>()
                .Select(type => GetExternalResourceGroup(type, sustainabilityData.Resources))
                .ToList();

            var sustainabilityResponse = new SustainabilityResponse(DateTime.UtcNow)
            {
                TotalSize = (sustainabilityData.PageWeight ?? 0) / 1024m,
                TotalEmissions = sustainabilityData.Emissions?.Co2 ?? 0,
                CarbonRating = sustainabilityData.CarbonRating,
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
        catch (Exception ex)
        {
            _eventLogService.LogException(nameof(SustainabilityService), nameof(RunNewReport), ex,
                $"Unexpected error while running sustainability report for URL: {url}");
            return null;
        }
    }

    public async Task<SustainabilityResponse?> GetLastReport(int webPageItemID, string languageName)
    {
        var sustainabilityPageDataInfo = (await _sustainabilityPageDataInfoProvider.Get()
            .WhereEquals(nameof(SustainabilityPageDataInfo.WebPageItemID), webPageItemID)
            .WhereEquals(nameof(SustainabilityPageDataInfo.LanguageName), languageName)
            .OrderByDescending(nameof(SustainabilityPageDataInfo.DateCreated))
            .TopN(1)
            .GetEnumerableTypedResultAsync()).FirstOrDefault();

        if (sustainabilityPageDataInfo is null)
        {
            return null;
        }

        var resourceGroups = JsonSerializer.Deserialize<List<ExternalResourceGroup>>(sustainabilityPageDataInfo.ResourceGroups);

        return new SustainabilityResponse(sustainabilityPageDataInfo.DateCreated)
        {
            TotalSize = sustainabilityPageDataInfo.TotalSize,
            TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
            CarbonRating = sustainabilityPageDataInfo.CarbonRating,
            ResourceGroups = resourceGroups ?? new List<ExternalResourceGroup>(),
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
        var resourcesByType = resources.Where(x => !string.IsNullOrEmpty(x.InitiatorType) && x.InitiatorType.Equals(initiator) && x.TransferSize > 0);

        var transferSize = 0;
        var resourceList = new List<ExternalResource>();
        foreach (var resource in resourcesByType.OrderByDescending(x => x.TransferSize))
        {
            if (string.IsNullOrEmpty(resource.Name))
            {
                continue;
            }

            transferSize += resource.TransferSize.GetValueOrDefault();
            resourceList.Add(new ExternalResource(resource.Name, (resource.TransferSize ?? 0) / 1024m));
        }

        return new ExternalResourceGroup(groupType)
        {
            TotalSize = transferSize / 1024m,
            Resources = resourceList
        };
    }
}
