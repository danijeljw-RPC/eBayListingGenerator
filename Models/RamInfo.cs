using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class RamInfo
{
    [JsonPropertyName("totalGiB")]
    public double? TotalGiB { get; set; }

    // Human-friendly (e.g. 16) while totalGiB can be 15.75
    [JsonPropertyName("readableGB")]
    public int? ReadableGB { get; set; }

    [JsonPropertyName("moduleCount")]
    public int? ModuleCount { get; set; }

    [JsonPropertyName("modules")]
    public List<RamModule> Modules { get; set; } = new();
}

public sealed class RamModule
{
    // Required to make modules useful (slot 0/1 etc)
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("capacityBytes")]
    public long? CapacityBytes { get; set; }

    [JsonPropertyName("capacityGiB")]
    public double? CapacityGiB { get; set; }

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("partNumber")]
    public string? PartNumber { get; set; }

    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("speedMHz")]
    public int? SpeedMHz { get; set; }

    [JsonPropertyName("configuredSpeedMHz")]
    public int? ConfiguredSpeedMHz { get; set; }

    [JsonPropertyName("deviceLocator")]
    public string? DeviceLocator { get; set; }

    [JsonPropertyName("bankLabel")]
    public string? BankLabel { get; set; }
}
