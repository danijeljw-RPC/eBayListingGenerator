using System.Text.RegularExpressions;
using EbayListingGenerator.Models;
using EbayListingGenerator.Parsing;

namespace EbayListingGenerator.Services;

public static class GpuExtractor
{
    private static readonly Regex GpuBlock = new(
        @"\[GPU\.(?<i>[0-9]+)\]\s*(?:\r?\n|\r)+(?<body>(?:.|\r|\n)*?)(?=(?:\r?\n|\r)\[GPU\.|(?:\r?\n|\r)\[Display\]|\Z)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private static readonly Regex Name = new(@"^Name\s*=\s*(?<v>.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex Vendor = new(@"^Vendor\s*=\s*(?<v>.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex Driver = new(@"^DriverVersion\s*=\s*(?<v>.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex VramGiB = new(@"^VramGiB\s*=\s*(?<v>[0-9.]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex VramBytes = new(@"^VramBytes\s*=\s*(?<v>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    public static List<GpuInfo> FromFullText(string fullText, Dictionary<string, string> normalized)
    {
        fullText ??= string.Empty;

        var list = new List<GpuInfo>();

        foreach (Match m in GpuBlock.Matches(fullText))
        {
            var body = m.Groups["body"].Value;

            var nameM = Name.Match(body);
            if (!nameM.Success) continue;

            var vendorM = Vendor.Match(body);
            var driverM = Driver.Match(body);
            var vramGiBM = VramGiB.Match(body);
            var vramBytesM = VramBytes.Match(body);

            double? vramGiB = null;

            if (vramGiBM.Success && double.TryParse(vramGiBM.Groups["v"].Value.Trim(), out var giB))
            {
                vramGiB = giB;
            }
            else if (vramBytesM.Success && long.TryParse(vramBytesM.Groups["v"].Value.Trim(), out var bytes) && bytes > 0)
            {
                // Deterministic conversion: bytes → GiB
                vramGiB = bytes / 1024d / 1024d / 1024d;
                vramGiB = Math.Round(vramGiB.Value, 2);
            }

            list.Add(new GpuInfo
            {
                Name = nameM.Groups["v"].Value.Trim(),
                Vendor = vendorM.Success ? NullIfEmpty(vendorM.Groups["v"].Value.Trim()) : null,
                DriverVersion = driverM.Success ? NullIfEmpty(driverM.Groups["v"].Value.Trim()) : null,
                VramGiB = vramGiB
            });
        }

        // Fallback: normalized only has GPU_Primary
        if (list.Count == 0)
        {
            var primary = NormalizedParser.StrOpt(normalized, "GPU_Primary");
            if (!string.IsNullOrWhiteSpace(primary))
                list.Add(new GpuInfo { Name = primary });
        }

        return list;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
