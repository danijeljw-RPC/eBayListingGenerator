using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class StorageInfo
{
    [JsonPropertyName("primary")]
    public StorageDisk Primary { get; set; } = new();

    [JsonPropertyName("systemDrive")]
    public SystemDriveInfo SystemDrive { get; set; } = new();
}

public sealed class StorageDisk
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("sizeGiB")]
    public double? SizeGiB { get; set; }

    // NVMe / SATA / HDD / Unknown
    [JsonPropertyName("interface")]
    public string? Interface { get; set; }

    [JsonPropertyName("interfaceHint")]
    public string? InterfaceHint { get; set; }
}

public sealed class SystemDriveInfo
{
    [JsonPropertyName("drive")]
    public string? Drive { get; set; }

    [JsonPropertyName("fileSystem")]
    public string? FileSystem { get; set; }

    [JsonPropertyName("sizeGiB")]
    public double? SizeGiB { get; set; }

    [JsonPropertyName("freeGiB")]
    public double? FreeGiB { get; set; }
}
