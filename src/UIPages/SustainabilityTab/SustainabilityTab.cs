using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Websites.UIPages;
using XperienceCommunity.Sustainability.Admin;
using XperienceCommunity.Sustainability.Models;
using XperienceCommunity.Sustainability.Services;

[assembly: UIPage(
    parentType: typeof(WebPageLayout),
    slug: "sustainability",
    uiPageType: typeof(SustainabilityTab),
    name: "Sustainability",
    templateName: "@sustainability/web-admin/SustainabilityTab",
    order: 20000,
    Icon = Icons.Earth)]

namespace XperienceCommunity.Sustainability.Admin;

public sealed class SustainabilityTab : WebPageBase<SustainabilityTabProperties>
{
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ISustainabilityService _sustainabilityService;
    private readonly IContentQueryExecutor _contentQueryExecutor;

    public SustainabilityTab(
        IWebPageManagerFactory webPageManagerFactory,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IPageLinkGenerator pageLinkGenerator,
        ISustainabilityService sustainabilityService,
        IWebPageUrlRetriever webPageUrlRetriever,
        IContentQueryExecutor contentQueryExecutor)
        : base(authenticatedUserAccessor, webPageManagerFactory, pageLinkGenerator)
    {
        _sustainabilityService = sustainabilityService;
        _webPageUrlRetriever = webPageUrlRetriever;
        _contentQueryExecutor = contentQueryExecutor;
    }

    public override async Task<SustainabilityTabProperties> ConfigureTemplateProperties(SustainabilityTabProperties properties)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(query =>
            {
                query.ForWebsite([WebPageIdentifier.WebPageItemID]);
            })
            .InLanguage(WebPageIdentifier.LanguageName);

        var currentPage = (await _contentQueryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(builder)).FirstOrDefault();

        // If the current page doesn't have IWebPageFieldsSource interface, we can't run the report. It's most likely a root page or a folder.
        if (currentPage is null)
        {
            properties.PageAvailability = PageAvailabilityStatus.NotAvailable;
            return properties;
        }

        properties.PageAvailability = PageAvailabilityStatus.Available;

        properties.SustainabilityData = await _sustainabilityService.GetLastReport(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);

        // Load initial historical reports (exclude the current report)
        var currentReportId = properties.SustainabilityData?.SustainabilityPageDataID;
        var (reports, hasMore) = await _sustainabilityService.GetReportHistory(
            WebPageIdentifier.WebPageItemID,
            WebPageIdentifier.LanguageName,
            excludeReportId: currentReportId,
            limit: 10,
            pageIndex: 0);
        properties.HistoricalReports = reports.ToList();
        properties.HasMoreHistory = hasMore;

        return properties;
    }

    [PageCommand]
    public async Task<SustainabilityResponseResult> RunReport()
    {
        var webPageUrl = await _webPageUrlRetriever.Retrieve(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);
        var absoluteUrl = webPageUrl.AbsoluteUrl;

        var sustainabilityData = await _sustainabilityService.RunNewReport(absoluteUrl, WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);

        if (sustainabilityData == null)
        {
            throw new InvalidOperationException("Failed to generate sustainability report. Check the event log for details.");
        }

        // Load updated historical reports (exclude the current report we just created)
        var currentReportId = sustainabilityData?.SustainabilityPageDataID;
        var (reports, hasMore) = await _sustainabilityService.GetReportHistory(
            WebPageIdentifier.WebPageItemID,
            WebPageIdentifier.LanguageName,
            excludeReportId: currentReportId,
            limit: 10,
            pageIndex: 0);

        return new SustainabilityResponseResult
        {
            SustainabilityData = sustainabilityData,
            HistoricalReports = reports.ToList(),
            HasMoreHistory = hasMore,
        };
    }

    [PageCommand]
    public async Task<HistoricalReportsResult> LoadMoreHistory(LoadMoreHistoryCommandData commandData)
    {
        // Get current report ID to exclude from history
        var currentReport = await _sustainabilityService.GetLastReport(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);
        var currentReportId = currentReport?.SustainabilityPageDataID;

        var (reports, hasMore) = await _sustainabilityService.GetReportHistory(
            WebPageIdentifier.WebPageItemID,
            WebPageIdentifier.LanguageName,
            excludeReportId: currentReportId,
            limit: 10,
            pageIndex: commandData.PageIndex);

        return new HistoricalReportsResult
        {
            HistoricalReports = reports.ToList(),
            HasMoreHistory = hasMore,
        };
    }

    [PageCommand]
    public async Task<PdfExportResult> ExportReportAsPdf()
    {
        var report = await _sustainabilityService.GetLastReport(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);

        if (report == null)
        {
            throw new InvalidOperationException("No sustainability report available to export.");
        }

        // Get the page name for the PDF
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(query =>
            {
                query.ForWebsite([WebPageIdentifier.WebPageItemID]);
            })
            .InLanguage(WebPageIdentifier.LanguageName);

        var currentPage = (await _contentQueryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(builder)).FirstOrDefault();
        // Use the page's display name (WebPageItemName) instead of ContentItemName (codename)
        var pageTitle = currentPage?.SystemFields.WebPageItemName ?? "Page";

        var webPageUrl = await _webPageUrlRetriever.Retrieve(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);
        var pdfBytes = await _sustainabilityService.GeneratePdfReport(report, pageTitle, webPageUrl.AbsoluteUrl);

        // Sanitize page title for filename and limit length
        var sanitizedTitle = string.Join("-", pageTitle.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        if (sanitizedTitle.Length > 50)
        {
            sanitizedTitle = sanitizedTitle.Substring(0, 50);
        }
        var fileName = $"sustainability-report-{sanitizedTitle}-{DateTime.Now:yyyy-MM-dd-HHmmss}.pdf";

        return new PdfExportResult
        {
            PdfBase64 = Convert.ToBase64String(pdfBytes),
            FileName = fileName
        };
    }
}

public readonly record struct SustainabilityResponseResult(SustainabilityResponse? SustainabilityData, List<SustainabilityResponse> HistoricalReports, bool HasMoreHistory);

public readonly record struct HistoricalReportsResult(List<SustainabilityResponse> HistoricalReports, bool HasMoreHistory);

public readonly record struct PdfExportResult
{
    public string PdfBase64 { get; init; }
    public string FileName { get; init; }
}

public class LoadMoreHistoryCommandData
{
    public int PageIndex { get; set; }
}

public sealed class SustainabilityTabProperties : TemplateClientProperties
{
    public PageAvailabilityStatus PageAvailability { get; set; }

    public SustainabilityResponse? SustainabilityData { get; set; }

    public List<SustainabilityResponse> HistoricalReports { get; set; } = [];

    public bool HasMoreHistory { get; set; }
}

public enum PageAvailabilityStatus
{
    Available,
    NotAvailable
}