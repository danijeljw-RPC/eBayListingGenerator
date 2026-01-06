using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class GpuInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("driverVersion")]
    public string? DriverVersion { get; set; }

    [JsonPropertyName("vramGiB")]
    public double? VramGiB { get; set; }
}
