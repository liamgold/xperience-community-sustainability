using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers.Internal;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.UIPages;
using System.Text.RegularExpressions;

namespace XperienceCommunity.Sustainability.Services;

/// <summary>
/// Service for generating links to Content Hub items from resource URLs
/// </summary>
public interface IContentHubLinkService
{
    /// <summary>
    /// Attempts to extract the Content Item GUID from a resource URL
    /// </summary>
    /// <param name="resourceUrl">The resource URL (e.g., /getcontentasset/...)</param>
    /// <returns>Content Item GUID if found, null otherwise</returns>
    Guid? TryExtractContentItemGuid(string resourceUrl);

    /// <summary>
    /// Generates a Content Hub admin URL for the given content item GUID
    /// </summary>
    /// <param name="contentItemGuid">The content item GUID</param>
    /// <param name="languageName">The language name for the admin URL</param>
    /// <returns>Content Hub admin URL if successful, null otherwise</returns>
    Task<string?> GenerateContentHubUrl(Guid contentItemGuid, string languageName);
}

public class ContentHubLinkService : IContentHubLinkService
{
    private const string CONTENT_ITEM_GROUP_NAME = "ContentItemGuid";

    // Matches /getcontentasset/{contentItemGuid}/{assetGuid}
    // Guid.TryParse() handles actual GUID validation, so we just capture word characters and hyphens
    private static readonly Regex contentItemLinkRegex = new(
        $@"\/getcontentasset\/(?<{CONTENT_ITEM_GROUP_NAME}>[\w-]+)\/[\w-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private readonly IEventLogService _eventLogService;
    private readonly IPageLinkGenerator _pageLinkGenerator;
    private readonly IContentItemManagerFactory _contentItemManagerFactory;
    private readonly IAuthenticatedUserAccessor _userAccessor;
    private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;

    public ContentHubLinkService(
        IEventLogService eventLogService,
        IPageLinkGenerator pageLinkGenerator,
        IContentItemManagerFactory contentItemManagerFactory,
        IAuthenticatedUserAccessor userAccessor,
        IInfoProvider<ContentItemInfo> contentItemInfoProvider)
    {
        _eventLogService = eventLogService;
        _pageLinkGenerator = pageLinkGenerator;
        _contentItemManagerFactory = contentItemManagerFactory;
        _userAccessor = userAccessor;
        _contentItemInfoProvider = contentItemInfoProvider;
    }

    public Guid? TryExtractContentItemGuid(string resourceUrl)
    {
        if (string.IsNullOrWhiteSpace(resourceUrl))
        {
            return null;
        }

        // Check if this is a content item asset URL
        var match = contentItemLinkRegex.Match(resourceUrl);
        if (!match.Success)
        {
            return null;
        }

        // Extract the content item GUID
        if (!Guid.TryParse(match.Groups[CONTENT_ITEM_GROUP_NAME].Value, out var contentItemGuid))
        {
            _eventLogService.LogWarning(
                nameof(ContentHubLinkService),
                "INVALID_GUID",
                $"Failed to parse GUID from: {match.Groups[CONTENT_ITEM_GROUP_NAME].Value}");
            return null;
        }

        return contentItemGuid;
    }

    public async Task<string?> GenerateContentHubUrl(Guid contentItemGuid, string languageName)
    {
        try
        {
            // Query using proper Kentico API - only select the column we need
            var results = await _contentItemInfoProvider
                .Get()
                .Columns(nameof(ContentItemInfo.ContentItemID))
                .WhereEquals(nameof(ContentItemInfo.ContentItemGUID), contentItemGuid)
                .TopN(1)
                .GetEnumerableTypedResultAsync();

            var contentItem = results.FirstOrDefault();

            if (contentItem == null)
            {
                _eventLogService.LogWarning(
                    nameof(ContentHubLinkService),
                    "NOT_FOUND",
                    $"Content item not found: GUID={contentItemGuid}");
                return null;
            }

            var contentItemId = contentItem.ContentItemID;

            // Get workspace ID from content item metadata using proper API
            var user = await _userAccessor.Get();
            var manager = _contentItemManagerFactory.Create(user.UserID);
            var metadata = await manager.GetContentItemMetadata(contentItemId, CancellationToken.None);

            var parameters = new PageParameterValues
            {
                { typeof(ContentHubWorkspace), metadata.WorkspaceId },
                { typeof(ContentHubContentLanguage), languageName },
                { typeof(ContentHubFolder), ContentHubSlugs.ALL_CONTENT_ITEMS },
                { typeof(ContentItemEditSection), contentItemId },
            };

            var contentItemPath = _pageLinkGenerator.GetPath<ContentItemEdit>(parameters);

            var url = $"/{AdminPathDefaults.ADMIN_PATH}{contentItemPath}";

            return url;
        }
        catch (Exception ex)
        {
            _eventLogService.LogException(
                nameof(ContentHubLinkService),
                "ERROR",
                ex,
                $"Error generating Content Hub URL for GUID={contentItemGuid}");
            return null;
        }
    }
}
