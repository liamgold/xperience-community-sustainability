using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class ExternalResource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; } = 0;

    /// <summary>
    /// Content Item GUID (stored in database for dynamic URL generation)
    /// </summary>
    [JsonPropertyName("contentItemGuid")]
    public Guid? ContentItemGuid { get; set; }

    /// <summary>
    /// Admin URL to view/edit this resource in Content Hub (generated dynamically, not stored)
    /// </summary>
    [JsonPropertyName("contentHubUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentHubUrl { get; set; }

    public ExternalResource() { }

    public ExternalResource(string url, decimal? size)
    {
        Url = url;
        Size = size;
    }
}
