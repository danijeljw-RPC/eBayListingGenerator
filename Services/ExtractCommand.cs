using System.Text.Json;
using EbayListingGenerator.Models;
using EbayListingGenerator.Parsing;
using EbayListingGenerator.Services;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Services;

public sealed class ExtractCommand
{
    public int Run(Argv argv)
    {
        var dir = argv.GetOpt("dir");
        var serial = argv.GetOpt("serial");
        var outPath = argv.GetOpt("out");

        if (string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(serial))
        {
            Console.Error.WriteLine("extract requires --dir and --serial");
            Console.Error.WriteLine(HelpText.Text);
            return 2;
        }

        dir = Path.GetFullPath(dir);
        var fullPath = Path.Combine(dir, $"{serial}.txt");
        var compactPath = Path.Combine(dir, $"{serial}_compact.txt");
        var normalizedPath = Path.Combine(dir, $"{serial}_normalized.txt");

        if (!File.Exists(fullPath))
            return Fail($"Missing input: {fullPath}");
        if (!File.Exists(compactPath))
            return Fail($"Missing input: {compactPath}");
        if (!File.Exists(normalizedPath))
            return Fail($"Missing input: {normalizedPath}");

        var full = File.ReadAllText(fullPath);
        var compact = File.ReadAllText(compactPath);
        var normalized = File.ReadAllText(normalizedPath);

        var listing = ListingBuilder.Build(serial!, normalized, full, compact);

        // Default output path
        if (string.IsNullOrWhiteSpace(outPath))
            outPath = Path.Combine(dir, $"{serial}.listing.json");

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);

        var json = JsonSerializer.Serialize(listing, JsonDefaults.OptionsIndented);
        File.WriteAllText(outPath, json);

        Console.WriteLine(outPath);
        return 0;
    }

    private static int Fail(string msg)
    {
        Console.Error.WriteLine(msg);
        return 2;
    }
}
