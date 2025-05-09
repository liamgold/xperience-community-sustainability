using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Websites.UIPages;
using XperienceCommunity.Sustainability.Admin;
using XperienceCommunity.Sustainability.Core.Services;

[assembly: UIPage(
    parentType: typeof(WebPageLayout),
    slug: "sustainability",
    uiPageType: typeof(SustainabilityTab),
    name: "Sustainability",
    templateName: "@sustainability/web-admin/SustainabilityTab",
    order: 700,
    Icon = Icons.Earth)]

namespace XperienceCommunity.Sustainability.Admin;

public sealed class SustainabilityTab : WebPageBase<SustainabilityTabProperties>
{
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ISustainabilityService _sustainabilityService;

    public SustainabilityTab(
        IWebPageManagerFactory webPageManagerFactory,
        IAuthenticatedUserAccessor authenticatedUserAccessor,
        IPageLinkGenerator pageLinkGenerator,
        ISustainabilityService sustainabilityService,
        IWebPageUrlRetriever webPageUrlRetriever)
        : base(authenticatedUserAccessor, webPageManagerFactory, pageLinkGenerator)
    {
        _sustainabilityService = sustainabilityService;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public override async Task<SustainabilityTabProperties> ConfigureTemplateProperties(SustainabilityTabProperties properties)
    {
        properties.Label = "Click the button to get sustainability data.";

        return properties;
    }

    [PageCommand]
    public async Task<ResponseResult> SetLabel()
    {
        var webPageUrl = await _webPageUrlRetriever.Retrieve(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);
        var absoluteUrl = webPageUrl.AbsoluteUrl;

        var sustainabilityData = await _sustainabilityService.GetSustainabilityData(absoluteUrl);

        return new ResponseResult
        {
            Label = sustainabilityData?.TotalEmissions != null
                ? $"TotalEmissions:{sustainabilityData.TotalEmissions:F4}g - recorded at {DateTime.UtcNow.ToShortTimeString()} - {Guid.NewGuid()}"
                : "No data"
        };
    }
}

public readonly record struct ResponseResult(string Label);

public sealed class SustainabilityTabProperties : TemplateClientProperties
{
    public string? Label { get; set; }
}
