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

        return new SustainabilityResponseResult
        {
            SustainabilityData = sustainabilityData,
        };
    }
}

public readonly record struct SustainabilityResponseResult(SustainabilityResponse? SustainabilityData);

public sealed class SustainabilityTabProperties : TemplateClientProperties
{
    public PageAvailabilityStatus PageAvailability { get; set; }

    public SustainabilityResponse? SustainabilityData { get; set; }
}

public enum PageAvailabilityStatus
{
    Available,
    NotAvailable
}