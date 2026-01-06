using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class SimpleItems
{
    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();
}
