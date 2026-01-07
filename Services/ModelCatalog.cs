using System.Text.Json;
using EbayListingGenerator.Models;

namespace EbayListingGenerator.Services;

public sealed class ModelCatalog
{
    private readonly Dictionary<string, ModelInfo> _models;

    private ModelCatalog(Dictionary<string, ModelInfo> models)
        => _models = models;

    public static ModelCatalog Load(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Missing config file: {path}");
            Environment.Exit(1);
        }

        var json = File.ReadAllText(path);

        var models = JsonSerializer.Deserialize<Dictionary<string, ModelInfo>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (models is null || models.Count == 0)
        {
            Console.Error.WriteLine("config.json is empty or invalid");
            Environment.Exit(1);
        }

        return new ModelCatalog(models);
    }

    public ModelInfo ResolveOrFail(string modelId)
    {
        if (_models.TryGetValue(modelId, out var info))
            return info;

        Console.Error.WriteLine($"Cannot find model '{modelId}'");
        Environment.Exit(1);
        return null!; // unreachable
    }
}
