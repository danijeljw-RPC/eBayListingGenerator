using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class WindowsInfo
{
    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("displayVersion")]
    public string? DisplayVersion { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("build")]
    public int? Build { get; set; }

    [JsonPropertyName("ubr")]
    public int? Ubr { get; set; }

    [JsonPropertyName("installedUtc")]
    public DateTime? InstalledUtc { get; set; }
}
