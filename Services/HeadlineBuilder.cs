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

        // RAM: keep TotalGiB in JSON, but title shows human-friendly GB (Format.GiBToHuman does that)
        var ram = l.Specs.Ram.TotalGiB is double r && r > 0.1
            ? $"{Format.GiBToHuman(r)} RAM"
            : "RAM Unknown";

        // Storage: marketed size mapping (238GiB -> 256GB) for headline readability
        var storageText = "Storage Unknown";
        if (l.Specs.Storage.Primary.SizeGiB is double s && s > 0.1)
        {
            var marketed = ToMarketedStorageGB(s);
            var kind = ShortStorageKind(l.Specs.Storage.Primary.Interface, l.Specs.Storage.Primary.InterfaceHint);
            storageText = kind is null ? $"{marketed}GB storage" : $"{marketed}GB {kind}";
        }

        var win = ShortWindowsTitle(l.Windows.Product);

        // Requested format: no serial in title
        return $"{manufacturer} (Model {model}) – {cpu} / {ram} / {storageText} / {win}";
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

        // Remove verbose Intel(R) Core(TM) noise but keep model + @ clock.
        var s = cpu.Replace("Intel(R) ", "", StringComparison.OrdinalIgnoreCase)
                   .Replace("Core(TM) ", "Core ", StringComparison.OrdinalIgnoreCase)
                   .Replace("CPU", "", StringComparison.OrdinalIgnoreCase)
                   .Replace("  ", " ", StringComparison.OrdinalIgnoreCase)
                   .Trim();

        return s;
    }

    private static string ShortWindowsTitle(string? product)
    {
        if (string.IsNullOrWhiteSpace(product))
            return "Windows";

        var p = product.Trim();

        // Win10 / Win11 shortening
        if (p.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
            return "Win10" + ExtractEdition(p, "Windows 10");
        if (p.StartsWith("Windows 11", StringComparison.OrdinalIgnoreCase))
            return "Win11" + ExtractEdition(p, "Windows 11");

        // fallback
        return p.Replace("Windows ", "Win ", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractEdition(string full, string prefix)
    {
        var rest = full.Substring(prefix.Length).Trim();
        if (string.IsNullOrWhiteSpace(rest)) return "";
        // e.g. "Home", "Pro"
        return " " + rest;
    }

    // Marketed sizes: map common GiB values to GB label users expect.
    private static int ToMarketedStorageGB(double sizeGiB)
    {
        // Common SSD/HDD marketed buckets (decimal GB labels)
        // 238 GiB -> 256GB, 476 GiB -> 512GB, etc.
        // Deterministic bucket mapping by GiB ranges.
        var g = sizeGiB;

        if (g >= 230 && g <= 245) return 256;
        if (g >= 450 && g <= 490) return 512;
        if (g >= 900 && g <= 990) return 1024;   // 1TB
        if (g >= 1800 && g <= 1980) return 2048; // 2TB

        // Otherwise, keep a sensible rounded number:
        return (int)Math.Round(g); // still stable and deterministic
    }

    private static string? ShortStorageKind(string? iface, string? hint)
    {
        var i = (iface ?? "").Trim();
        var h = (hint ?? "").Trim();

        // If you later set interface to NVMe/SATA/HDD in JSON, title will be correct automatically.
        if (i.Equals("NVMe", StringComparison.OrdinalIgnoreCase) || h.Contains("NVMe", StringComparison.OrdinalIgnoreCase))
            return "NVMe SSD";

        if (i.Equals("SATA", StringComparison.OrdinalIgnoreCase))
            return "SSD";

        if (i.Equals("HDD", StringComparison.OrdinalIgnoreCase))
            return "HDD";

        // Unknown: don't guess
        return null;
    }
}
