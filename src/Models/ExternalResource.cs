using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class ExternalResource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("size")]
    public decimal? Size { get; set; } = 0;

    public ExternalResource() { }

    public ExternalResource(string url, decimal? size)
    {
        Url = url;
        Size = size;
    }
}
