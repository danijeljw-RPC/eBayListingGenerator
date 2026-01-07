using EbayListingGenerator.Models;
using EbayListingGenerator.Parsing;

namespace EbayListingGenerator.Services;

public static class GpuExtractor
{
    public static List<GpuInfo> FromFullText(string fullText, Dictionary<string, string> normalized)
    {
        fullText ??= string.Empty;

        var ini = IniFile.Parse(fullText);

        var list = new List<GpuInfo>();

        var gpuRoot = ini.GetSection("GPU");
        var gpuCount = gpuRoot.GetInt("Count") ?? 0;

        if (gpuCount > 0)
        {
            for (int i = 0; i < gpuCount; i++)
            {
                var g = ini.GetSection($"GPU.{i}");
                var name = g.GetString("Name");
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var vramGiB = g.GetDouble("VramGiB");

                // Deterministic fallback: bytes -> GiB (2dp)
                if (vramGiB is null)
                {
                    var bytes = g.GetLong("VramBytes");
                    if (bytes is long b && b > 0)
                        vramGiB = Math.Round(b / 1024d / 1024d / 1024d, 2);
                }

                list.Add(new GpuInfo
                {
                    Name = name.Trim(),
                    Vendor = NullIfEmpty(g.GetString("Vendor")),
                    DriverVersion = NullIfEmpty(g.GetString("DriverVersion")),
                    VramGiB = vramGiB
                });
            }
        }

        // Fallback (only if full parsing produced nothing)
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
