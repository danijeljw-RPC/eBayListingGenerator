using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class WifiInfo
{
    [JsonPropertyName("generation")]
    public string? Generation { get; set; }

    // e.g. ["802.11b","802.11g",...]
    [JsonPropertyName("radios")]
    public List<string> Radios { get; set; } = new();

    [JsonPropertyName("driverVendor")]
    public string? DriverVendor { get; set; }

    [JsonPropertyName("driverVersion")]
    public string? DriverVersion { get; set; }
}
