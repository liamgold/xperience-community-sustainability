using System.Text.RegularExpressions;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.UIPages;

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
    private const string GUID_REGEX = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

    // Updated regex to handle both absolute URLs (https://...) and relative paths (/getcontentasset/...)
    private readonly Regex contentItemLinkRegex = new(
        @$"\/getcontentasset\/(?<{CONTENT_ITEM_GROUP_NAME}>{GUID_REGEX})\/{GUID_REGEX}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private readonly IEventLogService eventLogService;
    private readonly IPageLinkGenerator pageLinkGenerator;
    private readonly IContentItemManagerFactory contentItemManagerFactory;
    private readonly IAuthenticatedUserAccessor userAccessor;

    public ContentHubLinkService(
        IEventLogService eventLogService,
        IPageLinkGenerator pageLinkGenerator,
        IContentItemManagerFactory contentItemManagerFactory,
        IAuthenticatedUserAccessor userAccessor)
    {
        this.eventLogService = eventLogService;
        this.pageLinkGenerator = pageLinkGenerator;
        this.contentItemManagerFactory = contentItemManagerFactory;
        this.userAccessor = userAccessor;
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
            eventLogService.LogWarning(
                nameof(ContentHubLinkService),
                "INVALID_GUID",
                $"Failed to parse GUID from: {match.Groups[CONTENT_ITEM_GROUP_NAME].Value}");
            return null;
        }

        return contentItemGuid;
    }

    public async Task<string?> GenerateContentHubUrl(Guid contentItemGuid, string languageName)
    {
        eventLogService.LogInformation(
            nameof(ContentHubLinkService),
            "PROCESSING",
            $"Generating Content Hub URL for: GUID={contentItemGuid}, Language={languageName}");

        try
        {
            // Simple database query to get ContentItemID by GUID
            var parameters = new QueryDataParameters();
            parameters.Add("@ContentItemGUID", contentItemGuid);

            var query = "SELECT TOP 1 ContentItemID, ContentItemName FROM CMS_ContentItem WHERE ContentItemGUID = @ContentItemGUID";
            var dataSet = ConnectionHelper.ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);

            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
            {
                eventLogService.LogWarning(
                    nameof(ContentHubLinkService),
                    "NOT_FOUND",
                    $"Content item not found: GUID={contentItemGuid}");
                return null;
            }

            var row = dataSet.Tables[0].Rows[0];
            var contentItemId = Convert.ToInt32(row["ContentItemID"]);
            var contentItemName = row["ContentItemName"]?.ToString() ?? "Unknown";

            // Get workspace ID from content item metadata using proper API
            var user = await userAccessor.Get();
            var manager = contentItemManagerFactory.Create(user.UserID);
            var metadata = await manager.GetContentItemMetadata(contentItemId, CancellationToken.None);

            // Construct Content Hub URL
            // URL format: /admin/content-hub/{workspaceId}/{language}/all/list/{contentItemId}/content
            var url = $"/admin/content-hub/{metadata.WorkspaceId}/{languageName}/{ContentHubSlugs.ALL_CONTENT_ITEMS}/list/{contentItemId}/content";

            eventLogService.LogInformation(
                nameof(ContentHubLinkService),
                "SUCCESS",
                $"Generated Content Hub URL for item {contentItemId} (Name: {contentItemName}, Workspace: {metadata.WorkspaceId})");

            return url;
        }
        catch (Exception ex)
        {
            eventLogService.LogException(
                nameof(ContentHubLinkService),
                "ERROR",
                ex,
                $"Error generating Content Hub URL for GUID={contentItemGuid}");
            return null;
        }
    }
}
