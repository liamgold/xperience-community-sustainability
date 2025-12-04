using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class DashboardResponse
{
    [JsonPropertyName("summary")]
    public DashboardSummary Summary { get; set; } = new();

    [JsonPropertyName("pages")]
    public List<DashboardPageItem> Pages { get; set; } = new();
}
