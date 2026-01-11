using System.Text;
using eBayListingGenerator.Grading;
using eBayListingGenerator.Models;

namespace eBayListingGenerator.Rendering;

public sealed class ListingRenderer
{
    private readonly ConditionOptionsCatalog _catalog;
    private readonly ConditionGrader _grader;

    public ListingRenderer()
    {
        _catalog = ConditionOptionsCatalog.CreateDefaultExtensive();
        _grader = new ConditionGrader(_catalog);
    }

    public string Render(string templateHtml, ListingDocument doc)
    {
        var (overall, resolvedParts) = _grader.Resolve(doc.Condition);
        var palette = GradePalette.For(overall);

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TITLE"] = HtmlFragments.Html(doc.Headline.Title),
            ["SUBTITLE"] = HtmlFragments.Html(doc.Headline.Subtitle),

            ["QUICK_HIGHLIGHTS_LI"] = BuildQuickHighlights(doc),
            ["SPECIFICATIONS_LI"] = BuildSpecifications(doc),
            ["BATTERY_LI"] = BuildBattery(doc),
            ["MOBILE_DATA_LI"] = BuildMobileData(doc),
            ["PORTS_LI"] = BuildPorts(doc),
            ["DISPLAY_LI"] = BuildDisplay(doc),
            ["INCLUDED_LI"] = BuildIncluded(doc),
            ["NOTES_LI"] = BuildNotes(doc),

            ["CONDITION_BADGE_HTML"] = BuildConditionBadge(overall, palette),
            ["CONDITION_PARTS_LI"] = BuildConditionParts(resolvedParts, doc.Condition.Note),
            ["CONDITION_GUIDE_LI"] = BuildConditionGuide(overall),

            ["SHIPPING_HTML"] = BuildShipping(doc, palette)
        };

        return HtmlTemplateEngine.Apply(templateHtml, tokens);
    }

    private static string BuildQuickHighlights(ListingDocument doc)
    {
        var cpu = doc.Specs.Cpu.Name;
        var ram = $"{doc.Specs.MemoryGiB:0.#}GB RAM";
        var storage = $"{doc.Specs.Storage.SizeGiB:0.#}GB {doc.Specs.Storage.Type}"
                      + (string.IsNullOrWhiteSpace(doc.Specs.Storage.InterfaceHint) ? "" : $" ({doc.Specs.Storage.InterfaceHint})");
        var os = $"{doc.Specs.Windows.Product} ({doc.Specs.Windows.DisplayVersion})";

        var gpuText = doc.Specs.Gpus.Count == 0
            ? "Unknown"
            : string.Join(" + ", doc.Specs.Gpus.Select(g =>
                g.VramGiB is null ? g.Name : $"{g.Name} ({g.VramGiB:0.#}GB)"));

        return string.Join("", new[]
        {
            HtmlFragments.Li("CPU", cpu),
            HtmlFragments.Li("Memory", ram),
            HtmlFragments.Li("Graphics", gpuText),
            HtmlFragments.Li("Storage", storage),
            HtmlFragments.Li("OS", os)
        });
    }

    private static string BuildSpecifications(ListingDocument doc)
    {
        var s = doc.Specs;
        var lines = new List<string>
        {
            HtmlFragments.Li("Model code", doc.Identity.ModelCode),
            HtmlFragments.Li("Serial", doc.Identity.Serial),
            HtmlFragments.Li("Processor", s.Cpu.Name),
            HtmlFragments.Li("CPU cores / threads", $"{s.Cpu.Cores} / {s.Cpu.Threads}"),
        };

        if (s.Cpu.MaxClockMHz is not null)
            lines.Add(HtmlFragments.Li("CPU max MHz (reported)", s.Cpu.MaxClockMHz.Value.ToString()));

        lines.Add(HtmlFragments.Li("Memory", $"{s.MemoryGiB:0.#} GiB"));

        if (s.Gpus.Count > 0)
        {
            for (var i = 0; i < s.Gpus.Count; i++)
            {
                var g = s.Gpus[i];
                var gpuLabel = s.Gpus.Count == 1 ? "Graphics" : $"Graphics {i + 1}";
                var gpuVal = g.VramGiB is null ? g.Name : $"{g.Name} ({g.VramGiB:0.#}GB)";
                lines.Add(HtmlFragments.Li(gpuLabel, gpuVal));
            }
        }

        lines.Add(HtmlFragments.Li("Physical disk", $"{s.Storage.PhysicalDiskModel} | {s.Storage.SizeGiB:0.#} GiB"));
        lines.Add(HtmlFragments.Li("System drive",
            $"{s.Storage.SystemDrive.Drive} | {s.Storage.SystemDrive.Fs} | Size: {s.Storage.SystemDrive.SizeGiB:0.#} GiB | Free: {s.Storage.SystemDrive.FreeGiB:0.#} GiB"));

        var win = $"{s.Windows.Product} | {s.Windows.Version} (Build {s.Windows.Build}) | Installed {s.Windows.InstalledUtc?.ToString("yyyy-MM-dd") ?? "Unknown"}";
        lines.Add(HtmlFragments.Li("Windows", win));

        return string.Join("", lines);
    }

    private static string BuildBattery(ListingDocument doc)
    {
        var b = doc.Specs.Battery;
        var lines = new List<string>();

        if (b.DesignmWh is not null) lines.Add(HtmlFragments.Li("Design capacity", $"{b.DesignmWh:N0} mWh"));
        if (b.FullmWh is not null && b.HealthPct is not null)
            lines.Add(HtmlFragments.Li("Full charge capacity", $"{b.FullmWh:N0} mWh (~{b.HealthPct:0.#}% health vs design)"));
        if (b.Cycles is not null) lines.Add(HtmlFragments.Li("Cycle count", b.Cycles.Value.ToString()));

        return string.Join("", lines);
    }

    private static string BuildMobileData(ListingDocument doc)
    {
        var md = doc.ModelReference.MobileData;
        var lines = new List<string>
        {
            HtmlFragments.Li("WWAN", md.HasWwanPotential ? "Potential / optional on some configurations" : "Not expected"),
            HtmlFragments.Li("SIM slot", md.HasSimSlotPotential ? "Potential / optional on some configurations" : "Not expected")
        };

        foreach (var note in md.Notes)
            lines.Add($"<li>{HtmlFragments.Html(note)}</li>");

        return string.Join("", lines);
    }

    private static string BuildPorts(ListingDocument doc)
    {
        if (doc.ModelReference.Ports.Count == 0)
            return "<li><em>Ports list missing. (Populate modelReference.ports in JSON)</em></li>";

        return HtmlFragments.UlFromLines(doc.ModelReference.Ports);
    }

    private static string BuildDisplay(ListingDocument doc)
    {
        var d = doc.Specs.Display;
        var lines = new List<string>();

        if (d.SizeInches is not null) lines.Add(HtmlFragments.Li("Size", $"{d.SizeInches:0.#}\""));
        if (!string.IsNullOrWhiteSpace(d.CurrentResolution)) lines.Add(HtmlFragments.Li("Current resolution", d.CurrentResolution!));
        if (d.RefreshHz is not null) lines.Add(HtmlFragments.Li("Refresh", $"{d.RefreshHz} Hz"));

        lines.Add(HtmlFragments.Li("Touch (potential)", doc.ModelReference.HasTouchScreenPotential ? "Yes (confirm on unit)" : "No"));

        return string.Join("", lines);
    }

    private static string BuildIncluded(ListingDocument doc)
        => HtmlFragments.UlFromLines(doc.Included.Items);

    private static string BuildNotes(ListingDocument doc)
        => HtmlFragments.UlFromLines(doc.Notes.Items);

    private static string BuildConditionBadge(Grade overall, GradePalette palette)
    {
        var label = overall switch
        {
            Grade.Excellent => "Excellent",
            Grade.VeryGood => "Very Good",
            Grade.Good => "Good",
            Grade.Fair => "Fair",
            _ => "For parts / not working"
        };

        return $@"<span style=""background:{palette.BadgeBackground}; border:1px solid {palette.BadgeBorder}; color:{palette.BadgeText}; padding:2px 8px; border-radius:999px;"">{HtmlFragments.Html(label)}</span>";
    }

    private static string BuildConditionParts(ResolvedConditionParts parts, string? note)
    {
        var li = new List<string>
        {
            HtmlFragments.Li(parts.Lid.Label, parts.Lid.Text),
            HtmlFragments.Li(parts.Back.Label, parts.Back.Text),
            HtmlFragments.Li(parts.Sides.Label, parts.Sides.Text),
            HtmlFragments.Li(parts.Keyboard.Label, parts.Keyboard.Text),
            HtmlFragments.Li(parts.Trackpad.Label, parts.Trackpad.Text),
            HtmlFragments.Li(parts.Screen.Label, parts.Screen.Text)
        };

        if (!string.IsNullOrWhiteSpace(note))
            li.Add(HtmlFragments.Li("Note", note!));

        return string.Join("", li);
    }

    private static string BuildConditionGuide(Grade overall)
    {
        var guide = new[]
        {
            "<li><strong>Excellent:</strong> looks near-new; only tiny marks if any.</li>",
            "<li><strong>Very Good:</strong> light, normal-use wear (minor scuffs/small scratches).</li>",
            "<li><strong>Good:</strong> visible cosmetic wear (scratches/scuffs), but no significant paint loss; fully functional.</li>",
            "<li><strong>Fair:</strong> heavier cosmetic wear (e.g., paint chips/missing paint, more obvious marks); fully functional.</li>",
            "<li><strong>For parts / not working:</strong> faults present; sold for repair/parts only.</li>"
        };

        var overallLabel = overall switch
        {
            Grade.Excellent => "Excellent",
            Grade.VeryGood => "Very Good",
            Grade.Good => "Good",
            Grade.Fair => "Fair",
            _ => "For parts / not working"
        };

        return string.Join("", guide) +
           $@"<div style=""color:#555; margin-top:8px;"">This unit is graded <strong>{HtmlFragments.Html(overallLabel)}</strong>.</div>";
    }

    private static string BuildShipping(ListingDocument doc, GradePalette palette)
    {
        if (!doc.Shipping.Enabled)
            return "";

        var bullets = HtmlFragments.UlFromLines(doc.Shipping.Bullets);

        var sb = new StringBuilder();

        sb.AppendLine($@"<div style=""margin: 12px 0 0 0; padding:12px 14px; border-left:5px solid {palette.LeftAccentBorder}; background:#f6f9ff; border-radius:10px;"">");
        sb.AppendLine(@"  <div style=""font-weight:800; font-size:16px; margin-bottom:6px;"">Shipping &amp; Pickup</div>");
        sb.AppendLine(@"  <ul style=""margin:0; padding-left:18px;"">");
        sb.AppendLine(@"    <li><strong>Free Parcel Post</strong> (Australia Post)</li>");
        sb.AppendLine(@"    <li><strong>$23 flat fee Express Post</strong> (Australia Post)</li>");
        sb.AppendLine(@"    <li><strong>Local pickup available</strong> (Newtown, NSW)</li>");
        sb.AppendLine(@"  </ul>");
        sb.AppendLine(@"</div>");

        return sb.ToString();

        //         return $@"
        // <div style=""margin: 12px 0 0 0; padding:12px 14px; border-left:5px solid {palette.LeftAccentBorder}; background:#f6f9ff; border-radius:10px;"">
        //   <div style=""font-weight:800; font-size:16px; margin-bottom:6px;"">Shipping &amp; Pickup</div>
        //   <ul style=""margin:0; padding-left:18px;"">{bullets}</ul>
        // </div>";
    }
}
