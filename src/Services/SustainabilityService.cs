using CMS.Core;
using CMS.DataEngine;
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

    Task<IEnumerable<SustainabilityResponse>> GetReportHistory(int webPageItemID, string languageName, int limit = 10, int offset = 0);

    Task<SustainabilityResponse?> RunNewReport(string url, int webPageItemID, string languageName);
}

public class SustainabilityService : ISustainabilityService
{
    private readonly IEventLogService _eventLogService;
    private readonly IInfoProvider<SustainabilityPageDataInfo> _sustainabilityPageDataInfoProvider;
    private readonly IContentHubLinkService _contentHubLinkService;
    private readonly SustainabilityOptions _options;

    private static readonly string ScriptPath = GetScriptPath();
    private const string SustainabilityDataTestId = "sustainabilityData";

    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ico", ".avif"];
    private static readonly string[] FontExtensions = [".woff", ".woff2", ".ttf", ".otf", ".eot"];

    private static string GetScriptPath()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        return $"/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js?v={version}";
    }

    public SustainabilityService(
        IEventLogService eventLogService,
        IInfoProvider<SustainabilityPageDataInfo> sustainabilityPageDataInfoProvider,
        IContentHubLinkService contentHubLinkService,
        IOptions<SustainabilityOptions> options)
    {
        _eventLogService = eventLogService;
        _sustainabilityPageDataInfoProvider = sustainabilityPageDataInfoProvider;
        _contentHubLinkService = contentHubLinkService;
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

        // Capture console logs from the browser (opt-in for debugging)
        if (_options.EnableBrowserConsoleLogging)
        {
            page.Console += (_, msg) =>
            {
                _eventLogService.LogInformation(nameof(SustainabilityService), "BrowserConsole",
                    $"[{msg.Type}] {msg.Text}");
            };
        }

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

            if (_options.EnableBrowserConsoleLogging)
            {
                _eventLogService.LogInformation(nameof(SustainabilityService), nameof(RunNewReport),
                    $"Received data from page: {dataJson}");
            }

            if (string.IsNullOrWhiteSpace(dataJson))
            {
                return null;
            }

            var sustainabilityData = JsonSerializer.Deserialize<SustainabilityData>(dataJson);
            if (sustainabilityData?.Resources == null)
            {
                return null;
            }

            var resourceGroupTasks = Enum.GetValues<ResourceGroupType>()
                .Select(type => GetExternalResourceGroup(type, sustainabilityData.Resources, languageName));

            var resourceGroups = (await Task.WhenAll(resourceGroupTasks)).ToList();

            var sustainabilityResponse = new SustainabilityResponse(DateTime.UtcNow)
            {
                TotalSize = (sustainabilityData.PageWeight ?? 0) / 1024m,
                TotalEmissions = sustainabilityData.Emissions?.Co2?.Total ?? 0,
                CarbonRating = sustainabilityData.CarbonRating,
                GreenHostingStatus = sustainabilityData.GreenHostingStatus,
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

        // Regenerate Content Hub URLs from stored GUIDs (ensures URLs are current)
        if (resourceGroups != null)
        {
            foreach (var group in resourceGroups)
            {
                if (group.Resources == null)
                {
                    continue;
                }

                foreach (var resource in group.Resources)
                {
                    if (resource.ContentItemGuid.HasValue)
                    {
                        resource.ContentHubUrl = await _contentHubLinkService.GenerateContentHubUrl(
                            resource.ContentItemGuid.Value,
                            languageName);
                    }
                }
            }
        }

        return new SustainabilityResponse(sustainabilityPageDataInfo.DateCreated)
        {
            TotalSize = sustainabilityPageDataInfo.TotalSize,
            TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
            CarbonRating = sustainabilityPageDataInfo.CarbonRating,
            GreenHostingStatus = sustainabilityPageDataInfo.GreenHostingStatus,
            ResourceGroups = resourceGroups ?? [],
        };
    }

    public async Task<IEnumerable<SustainabilityResponse>> GetReportHistory(int webPageItemID, string languageName, int limit = 10, int offset = 0)
    {
        var sustainabilityPageDataInfos = await _sustainabilityPageDataInfoProvider.Get()
            .WhereEquals(nameof(SustainabilityPageDataInfo.WebPageItemID), webPageItemID)
            .WhereEquals(nameof(SustainabilityPageDataInfo.LanguageName), languageName)
            .OrderByDescending(nameof(SustainabilityPageDataInfo.DateCreated))
            .Page(offset, limit)
            .GetEnumerableTypedResultAsync();

        var responses = new List<SustainabilityResponse>();

        foreach (var sustainabilityPageDataInfo in sustainabilityPageDataInfos)
        {
            var resourceGroups = JsonSerializer.Deserialize<List<ExternalResourceGroup>>(sustainabilityPageDataInfo.ResourceGroups);

            // Regenerate Content Hub URLs from stored GUIDs (ensures URLs are current)
            if (resourceGroups != null)
            {
                foreach (var group in resourceGroups)
                {
                    if (group.Resources == null)
                    {
                        continue;
                    }

                    foreach (var resource in group.Resources)
                    {
                        if (resource.ContentItemGuid.HasValue)
                        {
                            resource.ContentHubUrl = await _contentHubLinkService.GenerateContentHubUrl(
                                resource.ContentItemGuid.Value,
                                languageName);
                        }
                    }
                }
            }

            responses.Add(new SustainabilityResponse(sustainabilityPageDataInfo.DateCreated)
            {
                TotalSize = sustainabilityPageDataInfo.TotalSize,
                TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
                CarbonRating = sustainabilityPageDataInfo.CarbonRating,
                GreenHostingStatus = sustainabilityPageDataInfo.GreenHostingStatus,
                ResourceGroups = resourceGroups ?? [],
            });
        }

        return responses;
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
            GreenHostingStatus = sustainabilityResponse.GreenHostingStatus,
            ResourceGroups = JsonSerializer.Serialize(sustainabilityResponse.ResourceGroups),
        };

        await _sustainabilityPageDataInfoProvider.SetAsync(infoObject);
    }

    private static bool IsImageFile(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Extract the path without query string
        var queryIndex = url.IndexOf('?');
        var pathOnly = queryIndex >= 0 ? url.Substring(0, queryIndex) : url;

        // Check common image extensions
        return ImageExtensions.Any(ext => pathOnly.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFontFile(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Extract the path without query string
        var queryIndex = url.IndexOf('?');
        var pathOnly = queryIndex >= 0 ? url.Substring(0, queryIndex) : url;

        // Check common font extensions
        return FontExtensions.Any(ext => pathOnly.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCssFile(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Extract the path without query string
        var queryIndex = url.IndexOf('?');
        var pathOnly = queryIndex >= 0 ? url.Substring(0, queryIndex) : url;

        // Check for CSS extension
        return pathOnly.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<ExternalResourceGroup> GetExternalResourceGroup(ResourceGroupType groupType, IList<Resource> resources, string languageName)
    {
        var initiator = ExternalResourceGroup.GetInitiatorType(groupType);

        // Filter resources - prioritize file extensions over initiatorType
        var resourcesByType = resources.Where(x =>
        {
            if (string.IsNullOrEmpty(x.InitiatorType) || x.TransferSize <= 0)
            {
                return false;
            }

            // Check file extension first (more reliable than initiatorType)
            var isImage = IsImageFile(x.Name);
            var isFont = IsFontFile(x.Name);
            var isCss = IsCssFile(x.Name);

            // Images: include image files regardless of initiatorType (e.g., CSS background images)
            if (groupType == ResourceGroupType.Images && isImage)
            {
                return true;
            }

            // Other: include font files (no dedicated Fonts category)
            if (groupType == ResourceGroupType.Other && isFont)
            {
                return true;
            }

            // CSS: include CSS files regardless of initiatorType
            if (groupType == ResourceGroupType.Css && isCss)
            {
                return true;
            }

            // Exclude files already categorized by extension from other groups
            if (isImage || isFont || isCss)
            {
                return false;
            }

            // Fallback to initiatorType for remaining resources
            return x.InitiatorType.Equals(initiator);
        });

        var transferSize = 0;
        var resourceList = new List<ExternalResource>();
        foreach (var resource in resourcesByType.OrderByDescending(x => x.TransferSize))
        {
            if (string.IsNullOrEmpty(resource.Name))
            {
                continue;
            }

            transferSize += resource.TransferSize.GetValueOrDefault();

            // Try to extract Content Item GUID and generate Content Hub URL
            var contentItemGuid = _contentHubLinkService.TryExtractContentItemGuid(resource.Name);
            string? contentHubUrl = null;

            if (contentItemGuid.HasValue)
            {
                contentHubUrl = await _contentHubLinkService.GenerateContentHubUrl(contentItemGuid.Value, languageName);
            }

            var externalResource = new ExternalResource(resource.Name, (resource.TransferSize ?? 0) / 1024m)
            {
                ContentItemGuid = contentItemGuid,
                ContentHubUrl = contentHubUrl
            };

            resourceList.Add(externalResource);
        }

        return new ExternalResourceGroup(groupType)
        {
            TotalSize = transferSize / 1024m,
            Resources = resourceList
        };
    }
}
