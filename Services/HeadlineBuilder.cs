using EbayListingGenerator.Models;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Services;

public static class HeadlineBuilder
{
    public static string BuildTitle(ListingRoot l)
    {
        var manufacturer = (l.Identity.Manufacturer ?? "Laptop").Trim();
        var model = (l.Identity.ModelCode ?? l.Identity.Model ?? "Unknown model").Trim();

        var cpu = ShortCpu(l.Specs.Cpu.Name);

        // Use BOTH: readable + precise (precise remains in JSON)
        var ram = "RAM Unknown";
        if (l.Specs.Ram.ReadableGB is int rgb && rgb > 0)
            ram = $"{rgb}GB RAM";
        else if (l.Specs.Ram.TotalGiB is double tg && tg > 0.1)
            ram = $"{Format.GiBToHuman(tg)} RAM";

        // Storage: marketed size (e.g. 238.5GiB => 256GB) + HDD label per your requirement
        var storage = "Storage Unknown";
        if (l.Specs.Storage.Primary.SizeGiB is double s && s > 0.1)
        {
            var marketed = ToMarketedStorageGB(s);
            storage = $"{marketed}GB HDD";
        }

        var win = ShortWindowsTitle(l.Windows.Product);

        // Required format: NO serial
        return $"{manufacturer} (Model {model}) – {cpu} / {ram} / {storage} / {win}";
    }

    public static string BuildSubtitle(ListingRoot l)
    {
        var bits = new List<string>();

        if (l.Specs.Display.SizeInches is double s && s > 0.1)
            bits.Add($"{s:0.#}\" display");

        if (!string.IsNullOrWhiteSpace(l.Specs.Display.MaxResolution))
            bits.Add($"max {l.Specs.Display.MaxResolution}");

        if (l.Specs.Gpus.Count > 0)
            bits.Add($"{l.Specs.Gpus.Count} GPU(s)");

        if (!string.IsNullOrWhiteSpace(l.Windows.DisplayVersion))
            bits.Add($"{l.Windows.Product} {l.Windows.DisplayVersion}");

        return bits.Count == 0 ? "Specs from provided system snapshot." : string.Join(" • ", bits);
    }

    private static string ShortCpu(string? cpu)
    {
        if (string.IsNullOrWhiteSpace(cpu)) return "CPU Unknown";

        // Remove verbose Intel(R) Core(TM) noise but keep model + @clock.
        var s = cpu.Replace("Intel(R) ", "", StringComparison.OrdinalIgnoreCase)
                   .Replace("Core(TM) ", "Core ", StringComparison.OrdinalIgnoreCase)
                   .Replace("CPU", "", StringComparison.OrdinalIgnoreCase)
                   .Trim();

        // collapse double spaces
        while (s.Contains("  ", StringComparison.Ordinal))
            s = s.Replace("  ", " ", StringComparison.Ordinal);

        return s;
    }

    private static string ShortWindowsTitle(string? product)
    {
        if (string.IsNullOrWhiteSpace(product))
            return "Windows";

        var p = product.Trim();

        if (p.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
            return "Win10" + ExtractEdition(p, "Windows 10");
        if (p.StartsWith("Windows 11", StringComparison.OrdinalIgnoreCase))
            return "Win11" + ExtractEdition(p, "Windows 11");

        return p.Replace("Windows ", "Win ", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractEdition(string full, string prefix)
    {
        var rest = full.Substring(prefix.Length).Trim();
        if (string.IsNullOrWhiteSpace(rest)) return "";
        return " " + rest;
    }

    // Deterministic marketed size mapping (GiB -> GB label users expect)
    private static int ToMarketedStorageGB(double sizeGiB)
    {
        var g = sizeGiB;

        if (g >= 230 && g <= 245) return 256;
        if (g >= 450 && g <= 490) return 512;
        if (g >= 900 && g <= 990) return 1024;
        if (g >= 1800 && g <= 1980) return 2048;

        return (int)Math.Round(g);
    }
}
