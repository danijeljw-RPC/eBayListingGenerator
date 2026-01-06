using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class Headline
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }
}
