using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class ListingRoot
{
    [JsonPropertyName("schema")]
    public string Schema { get; set; } = "ebay-listing-v1";

    [JsonPropertyName("generatedUtc")]
    public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("identity")]
    public Identity Identity { get; set; } = new();

    [JsonPropertyName("headline")]
    public Headline Headline { get; set; } = new();

    [JsonPropertyName("specs")]
    public Specs Specs { get; set; } = new();

    [JsonPropertyName("windows")]
    public WindowsInfo Windows { get; set; } = new();

    // Ports are intended to be edited by humans post-extract.
    [JsonPropertyName("ports")]
    public List<PortOption> Ports { get; set; } = new();

    [JsonPropertyName("modelReference")]
    public ModelReference ModelReference { get; set; } = new();

    [JsonPropertyName("battery")]
    public BatteryInfo? Battery { get; set; }

    [JsonPropertyName("included")]
    public SimpleItems Included { get; set; } = new();

    [JsonPropertyName("notes")]
    public SimpleItems Notes { get; set; } = new();

    [JsonPropertyName("condition")]
    public Condition Condition { get; set; } = new();
}
