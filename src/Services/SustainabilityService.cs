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

    Task<(IEnumerable<SustainabilityResponse> reports, bool hasMore)> GetReportHistory(int webPageItemID, string languageName, int? excludeReportId = null, int limit = 10, int pageIndex = 0);

    Task<SustainabilityResponse?> RunNewReport(string url, int webPageItemID, string languageName);

    Task<byte[]> GeneratePdfReport(SustainabilityResponse report, string pageTitle, string pageUrl);
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

            // Save to database and capture the newly created ID
            var newReportId = await LogSustainabilityResponse(sustainabilityResponse, webPageItemID, languageName);
            sustainabilityResponse.SustainabilityPageDataID = newReportId;

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
            SustainabilityPageDataID = sustainabilityPageDataInfo.SustainabilityPageDataID,
            TotalSize = sustainabilityPageDataInfo.TotalSize,
            TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
            CarbonRating = sustainabilityPageDataInfo.CarbonRating,
            GreenHostingStatus = sustainabilityPageDataInfo.GreenHostingStatus,
            ResourceGroups = resourceGroups ?? [],
        };
    }

    public async Task<(IEnumerable<SustainabilityResponse> reports, bool hasMore)> GetReportHistory(int webPageItemID, string languageName, int? excludeReportId = null, int limit = 10, int pageIndex = 0)
    {
        // Build query with optional filter to exclude a specific report (e.g., the current one)
        var query = _sustainabilityPageDataInfoProvider.Get()
            .WhereEquals(nameof(SustainabilityPageDataInfo.WebPageItemID), webPageItemID)
            .WhereEquals(nameof(SustainabilityPageDataInfo.LanguageName), languageName);

        // Exclude a specific report if specified (e.g., exclude current report from history)
        if (excludeReportId.HasValue)
        {
            query = query.WhereNotEquals(nameof(SustainabilityPageDataInfo.SustainabilityPageDataID), excludeReportId.Value);
        }

        // Get total count for hasMore calculation
        var totalCount = query.Count;

        var sustainabilityPageDataInfos = await query
            .OrderByDescending(nameof(SustainabilityPageDataInfo.DateCreated))
            .Page(pageIndex, limit)
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
                SustainabilityPageDataID = sustainabilityPageDataInfo.SustainabilityPageDataID,
                TotalSize = sustainabilityPageDataInfo.TotalSize,
                TotalEmissions = sustainabilityPageDataInfo.TotalEmissions,
                CarbonRating = sustainabilityPageDataInfo.CarbonRating,
                GreenHostingStatus = sustainabilityPageDataInfo.GreenHostingStatus,
                ResourceGroups = resourceGroups ?? [],
            });
        }

        // Calculate if there are more items beyond current page
        var itemsReturned = responses.Count;
        var itemsSoFar = (pageIndex * limit) + itemsReturned;
        var hasMore = itemsSoFar < totalCount;

        return (responses, hasMore);
    }

    private async Task<int> LogSustainabilityResponse(SustainabilityResponse sustainabilityResponse, int webPageItemID, string languageName)
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

        // Return the newly created ID
        return infoObject.SustainabilityPageDataID;
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

    public async Task<byte[]> GeneratePdfReport(SustainabilityResponse report, string pageTitle, string pageUrl)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        var htmlContent = GeneratePdfHtml(report, pageTitle, pageUrl);
        await page.SetContentAsync(htmlContent);

        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            PrintBackground = true,
            Margin = new Margin
            {
                Top = "1.5cm",
                Right = "1.5cm",
                Bottom = "1.5cm",
                Left = "1.5cm"
            }
        });

        return pdfBytes;
    }

    private string GeneratePdfHtml(SustainabilityResponse report, string pageTitle, string pageUrl)
    {
        var ratingColor = report.CarbonRating switch
        {
            "A+" => "#10b981",
            "A" => "#22c55e",
            "B" => "#84cc16",
            "C" => "#eab308",
            "D" => "#f97316",
            "E" => "#ef4444",
            "F" => "#dc2626",
            _ => "#6b7280"
        };

        var ratingDescription = report.CarbonRating switch
        {
            "A+" => "Extremely efficient",
            "A" => "Very efficient",
            "B" => "Efficient",
            "C" => "Moderate efficiency",
            "D" => "Low efficiency",
            "E" => "Poor efficiency",
            "F" => "Very poor efficiency",
            _ => "Not Rated"
        };

        var ratingSecondaryText = report.CarbonRating switch
        {
            "A+" or "A" => "This page has excellent carbon efficiency.",
            "B" or "C" => "This page has room for improvement.",
            "D" or "E" or "F" => "This page needs significant optimization.",
            _ => ""
        };

        var hostingBadge = report.GreenHostingStatus == "Green"
            ? "<span style=\"background: #d1fae5; color: #065f46; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">✓ Green Hosting</span>"
            : report.GreenHostingStatus == "NotGreen"
            ? "<span style=\"background: #fee2e2; color: #991b1b; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">Standard Hosting</span>"
            : "<span style=\"background: #f3f4f6; color: #4b5563; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">Unknown</span>";

        var totalResources = report.ResourceGroups?.Sum(g => g.Resources?.Count ?? 0) ?? 0;

        var resourceBreakdown = new System.Text.StringBuilder();
        if (report.ResourceGroups != null && report.ResourceGroups.Count > 0)
        {
            resourceBreakdown.Append("<div style=\"margin-top: 30px;\">");
            resourceBreakdown.Append("<h2 style=\"font-size: 18px; font-weight: 700; color: #111827; margin-bottom: 16px;\">Resource Breakdown</h2>");

            foreach (var group in report.ResourceGroups.OrderByDescending(g => g.TotalSize))
            {
                var topResources = group.Resources?.OrderByDescending(r => r.Size).Take(5).ToList();
                if (topResources == null || topResources.Count == 0) continue;

                resourceBreakdown.Append($@"
                <div class=""resource-group"" style=""margin-bottom: 20px; padding: 16px; background: #f9fafb; border-radius: 8px;"">
                    <div style=""font-weight: 600; color: #111827; margin-bottom: 8px;"">{group.Name}</div>
                    <div style=""font-size: 12px; color: #6b7280; margin-bottom: 12px;"">{group.Resources?.Count ?? 0} resources • {group.TotalSize:F2} KB total</div>
                    <table style=""width: 100%; border-collapse: collapse;"">
                ");

                foreach (var resource in topResources)
                {
                    var fileName = System.IO.Path.GetFileName(resource.Url) ?? resource.Url ?? "Unknown";
                    if (fileName.Length > 60) fileName = fileName.Substring(0, 57) + "...";

                    resourceBreakdown.Append($@"
                        <tr style=""border-bottom: 1px solid #e5e7eb;"">
                            <td style=""padding: 8px 0; font-size: 12px; color: #374151; width: 70%;"">{System.Security.SecurityElement.Escape(fileName)}</td>
                            <td style=""padding: 8px 0; font-size: 12px; color: #6b7280; text-align: right;"">{resource.Size:F2} KB</td>
                        </tr>
                    ");
                }

                resourceBreakdown.Append("</table></div>");
            }

            resourceBreakdown.Append("</div>");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #111827;
            padding: 20px;
        }}
        .resource-group {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
        .carbon-rating-hero {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
        .metrics-grid {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
    </style>
</head>
<body>
    <div style=""max-width: 800px; margin: 0 auto;"">
        <!-- Header -->
        <div style=""text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 2px solid #e5e7eb;"">
            <h1 style=""font-size: 28px; font-weight: 800; color: #111827; margin-bottom: 8px;"">Sustainability Report</h1>
            <div style=""font-size: 16px; color: #6b7280; margin-bottom: 4px;"">{System.Security.SecurityElement.Escape(pageTitle)}</div>
            <div style=""font-size: 12px; color: #9ca3af;"">{System.Security.SecurityElement.Escape(pageUrl)}</div>
            <div style=""font-size: 12px; color: #9ca3af; margin-top: 8px;"">Generated: {report.LastRunDate}</div>
        </div>

        <!-- Carbon Rating Hero -->
        <div class=""carbon-rating-hero"" style=""background: linear-gradient(135deg, {ratingColor}15 0%, white 100%); border: 2px solid {ratingColor}; border-radius: 12px; padding: 30px; margin-bottom: 30px; text-align: center;"">
            <div style=""font-size: 14px; font-weight: 600; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 12px;"">Carbon Rating</div>
            <div style=""font-size: 72px; font-weight: 900; color: {ratingColor}; margin-bottom: 12px; line-height: 1;"">{report.CarbonRating}</div>
            <div style=""font-size: 18px; font-weight: 600; color: #111827; margin-bottom: 8px;"">{ratingDescription}</div>
            <div style=""font-size: 14px; color: #6b7280; margin-bottom: 16px;"">{ratingSecondaryText}</div>
            {hostingBadge}
        </div>

        <!-- Metrics Grid -->
        <div class=""metrics-grid"" style=""display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 30px;"">
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">CO₂ Emissions</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{report.TotalEmissions:F3}g</div>
                <div style=""font-size: 12px; color: #9ca3af;"">per page view</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Page Weight</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{report.TotalSize:F2}KB</div>
                <div style=""font-size: 12px; color: #9ca3af;"">{(report.TotalSize / 1024):F2} MB total</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Resources</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{totalResources}</div>
                <div style=""font-size: 12px; color: #9ca3af;"">{report.ResourceGroups?.Count ?? 0} categories</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Methodology</div>
                <div style=""font-size: 16px; font-weight: 700; color: #111827; margin-bottom: 4px;"">SWDM v4</div>
                <div style=""font-size: 12px; color: #9ca3af;"">Sustainable Web Design</div>
            </div>
        </div>

        {resourceBreakdown}

        <!-- Footer -->
        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 11px; color: #9ca3af;"">
            <div>Powered by <strong><a href=""https://github.com/liamgold/xperience-community-sustainability"" style=""color: #111827; text-decoration: none;"">Xperience Community: Sustainability</a></strong></div>
            <div style=""margin-top: 4px;"">Carbon ratings based on Sustainable Web Design Model v4</div>
            <div style=""margin-top: 4px;"">https://sustainablewebdesign.org/digital-carbon-ratings/</div>
        </div>
    </div>
</body>
</html>";
    }
}
