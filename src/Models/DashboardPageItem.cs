using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class DashboardPageItem
{
    [JsonPropertyName("webPageItemID")]
    public int WebPageItemID { get; set; }

    [JsonPropertyName("pageName")]
    public string PageName { get; set; } = string.Empty;

    [JsonPropertyName("pageUrl")]
    public string PageUrl { get; set; } = string.Empty;

    [JsonPropertyName("languageName")]
    public string LanguageName { get; set; } = string.Empty;

    [JsonPropertyName("carbonRating")]
    public string? CarbonRating { get; set; }

    [JsonPropertyName("totalEmissions")]
    public double TotalEmissions { get; set; }

    [JsonPropertyName("totalSize")]
    public decimal TotalSize { get; set; }

    [JsonPropertyName("greenHostingStatus")]
    public string? GreenHostingStatus { get; set; }

    [JsonPropertyName("lastRunDate")]
    public string LastRunDate { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime DateCreated { get; set; }
}
