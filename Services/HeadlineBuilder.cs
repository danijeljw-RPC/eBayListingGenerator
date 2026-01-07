using EbayListingGenerator.Models;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Services;

public static class HeadlineBuilder
{
    public static string BuildTitle(
        ListingRoot l,
        ModelCatalog modelCatalog
    )
    {
        var modelId = (l.Identity.ModelCode ?? l.Identity.Model)?.Trim();

        if (string.IsNullOrWhiteSpace(modelId))
        {
            Console.Error.WriteLine(
                "Missing model identifier in listing identity"
            );
            Environment.Exit(1);
        }

        var modelInfo = modelCatalog.ResolveOrFail(modelId);

        var manufacturer = modelInfo.Manufacturer.Trim();
        var modelGenericName = modelInfo.ModelGenericName.Trim();

        var cpu = ShortCpu(l.Specs.Cpu.Name);

        var ram = "RAM Unknown";
        if (l.Specs.Ram.ReadableGB is int rgb && rgb > 0)
            ram = $"{rgb}GB RAM";
        else if (l.Specs.Ram.TotalGiB is double tg && tg > 0.1)
            ram = $"{Format.GiBToHuman(tg)} RAM";

        var storage = "Storage Unknown";
        if (l.Specs.Storage.Primary.SizeGiB is double s && s > 0.1)
        {
            var marketed = ToMarketedStorageGB(s);
            storage = $"{marketed}GB HDD";
        }

        var win = ShortWindowsTitle(l.Windows.Product);

        return
            $"{manufacturer} {modelGenericName} (Model {modelId}) – " +
            $"{cpu} / {ram} / {storage} / {win}";
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

        return bits.Count == 0
            ? "Specs from provided system snapshot."
            : string.Join(" • ", bits);
    }

    private static string ShortCpu(string? cpu)
    {
        if (string.IsNullOrWhiteSpace(cpu)) return "CPU Unknown";

        var s = cpu.Replace("Intel(R) ", "", StringComparison.OrdinalIgnoreCase)
                   .Replace("Core(TM) ", "Core ", StringComparison.OrdinalIgnoreCase)
                   .Replace("CPU", "", StringComparison.OrdinalIgnoreCase)
                   .Trim();

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

    private static int ToMarketedStorageGB(double sizeGiB)
    {
        if (sizeGiB >= 230 && sizeGiB <= 245) return 256;
        if (sizeGiB >= 450 && sizeGiB <= 490) return 512;
        if (sizeGiB >= 900 && sizeGiB <= 990) return 1024;
        if (sizeGiB >= 1800 && sizeGiB <= 1980) return 2048;

        return (int)Math.Round(sizeGiB);
    }
}
