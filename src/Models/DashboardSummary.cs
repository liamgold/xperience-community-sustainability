using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class DashboardSummary
{
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("averageEmissions")]
    public double AverageEmissions { get; set; }

    [JsonPropertyName("averagePageWeight")]
    public decimal AveragePageWeight { get; set; }

    [JsonPropertyName("ratingDistribution")]
    public Dictionary<string, int> RatingDistribution { get; set; } = new();

    [JsonPropertyName("greenHostingCount")]
    public int GreenHostingCount { get; set; }
}
