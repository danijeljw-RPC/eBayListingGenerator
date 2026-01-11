using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class Condition
{
    [JsonPropertyName("forPartsNotWorking")]
    public bool ForPartsNotWorking { get; set; } = false;

    [JsonPropertyName("grade")]
    public string Grade { get; set; } = "Good"; // Excellent/Very Good/Good/Fair/For parts

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("parts")]
    public Dictionary<string, string> Parts { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lid"] = "Good",
        ["back"] = "Good",
        ["sides"] = "Good",
        ["keyboard"] = "Good",
        ["trackpad"] = "Good",
        ["screen"] = "Good"
    };

    // Render-only styling (not part of the JSON schema)
    [JsonIgnore] public string GradeBg { get; set; } = "#f1f8e9";
    [JsonIgnore] public string GradeBorder { get; set; } = "#558b2f";
    [JsonIgnore] public string GradeText { get; set; } = "#33691e";
}
