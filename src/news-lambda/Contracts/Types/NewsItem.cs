using System.Text.Json.Serialization;

namespace Contracts.Types;

public class NewsLinkItem
{
    [JsonPropertyName("publisher")]
    public required string Publisher { get; set; } // Changed to List<string> to match JSON array

    [JsonPropertyName("link")]
    public required string Link { get; set; } // Changed to List<string> to match JSON array
}

public class NewsItem
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("links")]
    public required List<NewsLinkItem> Links { get; set; }
}
