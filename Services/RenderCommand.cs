using System.Text.Json;
using EbayListingGenerator.Models;
using EbayListingGenerator.Rendering;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Services;

public sealed class RenderCommand
{
    public int Run(Argv argv)
    {
        var jsonPath     = argv.GetOpt("json") ?? argv.GetOpt("input");
        var outPath      = argv.GetOpt("out");
        var templateFile = argv.GetOpt("template-file");
        var templateName = argv.GetOpt("template");

        if (string.IsNullOrWhiteSpace(jsonPath) || string.IsNullOrWhiteSpace(outPath))
            return Fail("render requires --json/--input and --out");

        if (templateFile is null && templateName is null)
            return Fail("render requires --template-file or --template");

        jsonPath = Path.GetFullPath(jsonPath);
        outPath  = Path.GetFullPath(outPath);

        var templatePath = ResolveTemplate(templateFile, templateName);

        var json = File.ReadAllText(jsonPath);
        var data = JsonSerializer.Deserialize<ListingRoot>(json)!;

        GradeCalculator.ApplyOverallGrade(data);

        var html = File.ReadAllText(templatePath);
        html = HtmlRenderer.Render(html, data);

        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, html);

        return 0;
    }

    private static string ResolveTemplate(string? file, string? name)
    {
        if (!string.IsNullOrWhiteSpace(file))
            return Path.GetFullPath(file);

        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Templates", $"{name}.html");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Template not found: {path}");

        return path;
    }

    private static int Fail(string msg)
    {
        Console.Error.WriteLine(msg);
        return 2;
    }
}
