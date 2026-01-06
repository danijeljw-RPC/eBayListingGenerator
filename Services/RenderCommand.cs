using System.Text.Json;
using EbayListingGenerator.Models;
using EbayListingGenerator.Rendering;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Services;

public sealed class RenderCommand
{
    public int Run(Argv argv)
    {
        var jsonPath = argv.GetOpt("json");
        var templatePath = argv.GetOpt("template");
        var outPath = argv.GetOpt("out");

        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            Console.Error.WriteLine("render requires --json");
            Console.Error.WriteLine(HelpText.Text);
            return 2;
        }

        jsonPath = Path.GetFullPath(jsonPath);
        if (!File.Exists(jsonPath))
            return Fail($"Missing input: {jsonPath}");

        var json = File.ReadAllText(jsonPath);
        var listing = JsonSerializer.Deserialize<ListingRoot>(json, JsonDefaults.Options) 
                      ?? throw new InvalidOperationException("Failed to parse listing JSON.");

        string templateHtml;
        if (!string.IsNullOrWhiteSpace(templatePath))
        {
            templatePath = Path.GetFullPath(templatePath);
            if (!File.Exists(templatePath))
                return Fail($"Missing template: {templatePath}");
            templateHtml = File.ReadAllText(templatePath);
        }
        else
        {
            // embedded file copied to output during build/publish? We'll just read from repo path relative to exe.
            templateHtml = EmbeddedTemplate.LoadDefaultTemplate();
        }

        var html = HtmlRenderer.Render(templateHtml, listing);

        if (string.IsNullOrWhiteSpace(outPath))
        {
            var dir = Path.GetDirectoryName(jsonPath)!;
            var serial = listing.Identity.Serial ?? Path.GetFileNameWithoutExtension(jsonPath);
            outPath = Path.Combine(dir, $"{serial}.html");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
        File.WriteAllText(outPath, html);

        Console.WriteLine(outPath);
        return 0;
    }

    private static int Fail(string msg)
    {
        Console.Error.WriteLine(msg);
        return 2;
    }
}
