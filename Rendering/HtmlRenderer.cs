using System.Text;
using EbayListingGenerator.Models;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator.Rendering;

public static class HtmlRenderer
{
    public static string Render(string templateHtml, ListingRoot listing)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{{TITLE}}"] = Html.Escape(listing.Headline.Title ?? "Untitled listing"),
            ["{{SUBTITLE}}"] = Html.Escape(listing.Headline.Subtitle ?? string.Empty),

            ["{{SECTION_QUICK_HIGHLIGHTS}}"] = BuildQuickHighlights(listing),
            ["{{SECTION_SPECS_THIS_UNIT}}"] = BuildSpecsThisUnit(listing),
            ["{{SECTION_WIFI}}"] = BuildWifi(listing),
            ["{{SECTION_DISPLAY}}"] = BuildDisplay(listing),
            ["{{SECTION_BATTERY}}"] = BuildBattery(listing),
            ["{{SECTION_MOBILE_DATA}}"] = BuildMobileData(listing),
            ["{{SECTION_PORTS}}"] = BuildPorts(listing),
            ["{{SECTION_INCLUDED}}"] = BuildSimpleSection("What's Included", listing.Included.Items),
            ["{{SECTION_NOTES}}"] = BuildSimpleSection("Notes", listing.Notes.Items),
            ["{{SECTION_CONDITION}}"] = BuildCondition(listing),
            ["{{SECTION_SHIPPING}}"] = string.Empty,
        };

        var output = templateHtml;
        foreach (var kvp in tokens)
            output = output.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);

        return output;
    }

    private static string BuildQuickHighlights(ListingRoot l)
    {
        // Only render if we can show at least 2 meaningful items
        var items = new List<string>();

        if (!string.IsNullOrWhiteSpace(l.Specs.Cpu.Name))
            items.Add(LiStrong("CPU", l.Specs.Cpu.Name!));

        if (l.Specs.Ram.TotalGiB is double ram && ram > 0.1)
            items.Add(LiStrong("Memory", $"{Format.GiBToHuman(ram)} RAM"));

        if (l.Specs.Gpus.Count > 0)
        {
            var gpuText = string.Join(" + ", l.Specs.Gpus
                .Where(g => !string.IsNullOrWhiteSpace(g.Name))
                .Select(g => g.VramGiB is double v && v > 0 ? $"{g.Name} ({Format.GiBToHuman(v)} VRAM)" : g.Name!)
            );
            if (!string.IsNullOrWhiteSpace(gpuText))
                items.Add(LiStrong("Graphics", gpuText));
        }

        if (l.Specs.Storage.Primary.SizeGiB is double s && s > 0.1)
        {
            var iface = string.IsNullOrWhiteSpace(l.Specs.Storage.Primary.Interface) ? "Storage" : l.Specs.Storage.Primary.Interface;
            items.Add(LiStrong("Storage", $"{Format.GiBToHuman(s)} {Html.Escape(iface)}"));
        }

        if (!string.IsNullOrWhiteSpace(l.Windows.Product))
        {
            var win = l.Windows.Product!;
            if (l.Windows.Build is int b && b > 0)
                win += $" (Build {b})";
            items.Add(LiStrong("OS", win));
        }

        if (items.Count < 2)
            return string.Empty;

        return Section("Quick Highlights", Ul(items));
    }

    private static string BuildSpecsThisUnit(ListingRoot l)
    {
        var li = new List<string>();

        if (!string.IsNullOrWhiteSpace(l.Identity.ModelCode))
            li.Add(LiStrong("Model code", l.Identity.ModelCode!));
        if (!string.IsNullOrWhiteSpace(l.Identity.Serial))
            li.Add(LiStrong("Serial", l.Identity.Serial!));

        if (!string.IsNullOrWhiteSpace(l.Specs.Cpu.Name))
            li.Add(LiStrong("Processor", l.Specs.Cpu.Name!));
        if (l.Specs.Cpu.Cores is int c && l.Specs.Cpu.Threads is int t)
            li.Add(LiStrong("CPU cores / threads", $"{c} / {t}"));
        if (l.Specs.Cpu.MaxClockMHz is int mhz && mhz > 0)
            li.Add(LiStrong("CPU max MHz (reported)", mhz.ToString()));

        if (l.Specs.Ram.TotalGiB is double ram && ram > 0.1)
            li.Add(LiStrong("Memory", $"{ram:0.##} GiB"));

        if (l.Specs.Gpus.Count > 0)
        {
            int idx = 1;
            foreach (var g in l.Specs.Gpus.Where(g => !string.IsNullOrWhiteSpace(g.Name)))
            {
                var label = l.Specs.Gpus.Count == 1 ? "Graphics" : $"Graphics {idx}";
                var val = g.VramGiB is double v && v > 0
                    ? $"{g.Name} ({v:0.##} GiB)"
                    : g.Name!;
                li.Add(LiStrong(label, val));
                idx++;
            }
        }

        if (!string.IsNullOrWhiteSpace(l.Specs.Storage.Primary.Model) || (l.Specs.Storage.Primary.SizeGiB is double ps && ps > 0.1))
        {
            var disk = new List<string>();
            if (!string.IsNullOrWhiteSpace(l.Specs.Storage.Primary.Model))
                disk.Add(l.Specs.Storage.Primary.Model!);
            if (l.Specs.Storage.Primary.SizeGiB is double size && size > 0.1)
                disk.Add($"{size:0.##} GiB");
            li.Add(LiStrong("Physical disk", string.Join(" | ", disk)));
        }

        if (!string.IsNullOrWhiteSpace(l.Specs.Storage.SystemDrive.Drive))
        {
            var parts = new List<string>
            {
                $"{l.Specs.Storage.SystemDrive.Drive}",
                $"{l.Specs.Storage.SystemDrive.FileSystem ?? "Unknown FS"}",
            };

            if (l.Specs.Storage.SystemDrive.SizeGiB is double sz && sz > 0.1)
                parts.Add($"Size: {sz:0.##} GiB");
            if (l.Specs.Storage.SystemDrive.FreeGiB is double fr && fr >= 0)
                parts.Add($"Free: {fr:0.##} GiB");

            li.Add(LiStrong("System drive", string.Join(" | ", parts)));
        }

        if (!string.IsNullOrWhiteSpace(l.Windows.Product))
        {
            var winParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(l.Windows.Product)) winParts.Add(l.Windows.Product!);
            if (!string.IsNullOrWhiteSpace(l.Windows.Version)) winParts.Add(l.Windows.Version!);
            if (l.Windows.Build is int b && b > 0) winParts.Add($"Build {b}{(l.Windows.Ubr is int ubr && ubr > 0 ? "." + ubr : "")}");
            if (l.Windows.InstalledUtc is DateTime dt) winParts.Add($"Installed {dt:yyyy-MM-dd}");
            li.Add(LiStrong("Windows", string.Join(" | ", winParts)));
        }

        if (li.Count == 0) return string.Empty;
        return Section("Specifications (This Unit)", Ul(li));
    }

    private static string BuildWifi(ListingRoot l)
    {
        var w = l.Specs.Wifi;
        var li = new List<string>();

        if (!string.IsNullOrWhiteSpace(w.Generation))
            li.Add(LiStrong("Generation", w.Generation!));

        if (w.Radios.Count > 0)
            li.Add(LiStrong("Radios", string.Join(" ", w.Radios.Select(Html.Escape))));

        var driver = new List<string>();
        if (!string.IsNullOrWhiteSpace(w.DriverVendor)) driver.Add(w.DriverVendor!);
        if (!string.IsNullOrWhiteSpace(w.DriverVersion)) driver.Add($"v{w.DriverVersion}");
        if (driver.Count > 0)
            li.Add(LiStrong("Driver", string.Join(" ", driver.Select(Html.Escape))));

        if (li.Count == 0) return string.Empty;

        return Section("Wi-Fi", Ul(li));
    }

    private static string BuildDisplay(ListingRoot l)
    {
        var d = l.Specs.Display;
        var li = new List<string>();

        if (d.SizeInches is double s && s > 0.1)
            li.Add(LiStrong("Size", $"{s:0.#}"));

        if (!string.IsNullOrWhiteSpace(d.MaxResolution))
            li.Add(LiStrong("Max resolution", d.MaxResolution!));

        if (d.RefreshHz is int hz && hz > 0)
            li.Add(LiStrong("Refresh", $"{hz} Hz"));

        if (l.ModelReference.HasTouchScreenPotential is bool touch)
            li.Add(LiStrong("Touch (potential)", touch ? "Yes" : "No"));

        if (li.Count == 0) return string.Empty;
        return Section("Display", Ul(li));
    }

    private static string BuildBattery(ListingRoot l)
    {
        if (l.Battery is null) return string.Empty;

        var b = l.Battery;
        var li = new List<string>();

        if (b.DesignCapacitymWh is int d && d > 0)
            li.Add(LiStrong("Design capacity", $"{d:n0} mWh"));

        if (b.FullChargeCapacitymWh is int f && f > 0)
        {
            var health = b.HealthPct is double hp && hp > 0 ? $" (~{hp:0.#}% health vs design)" : string.Empty;
            li.Add(LiStrong("Full charge capacity", $"{f:n0} mWh{health}"));
        }

        if (b.CycleCount is int c && c >= 0)
            li.Add(LiStrong("Cycle count", c.ToString()));

        if (li.Count == 0) return string.Empty;

        var note = @"<div style=""color:#555; font-size:13px; margin-top:6px;"">
Battery health is indicative only and reflects current measured capacity at time of testing.
</div>";

        return Section("Battery Health", Ul(li) + note);
    }

    private static string BuildMobileData(ListingRoot l)
    {
        var md = l.ModelReference.MobileData;
        if (md is null) return string.Empty;

        var hasSim = md.HasSimSlotPotential is true;
        var hasWwan = md.HasWwanPotential is true;

        if (!hasSim && !hasWwan)
            return string.Empty; // omit entire section

        var li = new List<string>();
        li.Add(LiStrong("WWAN (potential)", hasWwan ? "Yes (config dependent)" : "No / not expected"));
        li.Add(LiStrong("SIM slot (potential)", hasSim ? "Yes (config dependent)" : "No / not expected"));

        if (!string.IsNullOrWhiteSpace(md.Notes))
            li.Add(LiStrong("Notes", md.Notes!));

        var note = @"<div style=""color:#555; font-size:13px; margin-top:6px;"">
WWAN / SIM support is model-dependent and may require antennas, tray, and card not fitted to this unit.
</div>";

        return Section("Mobile Data (Model Reference)", Ul(li) + note);
    }

    private static string BuildPorts(ListingRoot l)
    {
        var enabled = l.Ports.Where(p => p.Enabled && !string.IsNullOrWhiteSpace(p.Name)).ToList();
        if (enabled.Count == 0) return string.Empty;

        var li = enabled.Select(p =>
        {
            var count = p.Count <= 1 ? "" : $"{p.Count}× ";
            var notes = string.IsNullOrWhiteSpace(p.Notes) ? "" : $" <span style=\"color:#555;\">({Html.Escape(p.Notes!)})</span>";
            return $"<li>{Html.Escape(count)}{Html.Escape(p.Name)}{notes}</li>";
        }).ToList();

        return Section("Ports & Connectivity", Ul(li));
    }

    private static string BuildSimpleSection(string title, List<string> items)
    {
        var clean = items.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (clean.Count == 0) return string.Empty;
        return Section(title, Ul(clean.Select(Html.Escape).Select(x => $"<li>{x}</li>").ToList()));
    }

    private static string BuildCondition(ListingRoot l)
    {
        // Always render (you'll likely want it even if basic)
        var grade = Html.Escape(l.Condition.Grade);
        var c = l.Condition;

        var badge = $@"<span style=""padding:2px 10px; border-radius:999px; font-weight:600; display:inline-block; background:{Html.Escape(c.GradeBg)}; border:1px solid {Html.Escape(c.GradeBorder)}; color:{Html.Escape(c.GradeText)};"">{grade}</span>";


        var parts = new StringBuilder();
        parts.AppendLine(@"<div style=""margin-top:20px; padding:14px; border:1px solid #eaeaea; border-radius:10px; background:#fcfcfc;"">");
        parts.AppendLine($@"  <div style=""font-weight:800; font-size:15px; margin-bottom:6px;"">Overall Condition Grade: {badge}</div>");
        parts.AppendLine(@"  <div style=""margin-top:10px;"">");
        parts.AppendLine(@"    <div style=""font-weight:800; margin-bottom:6px;"">Cosmetic condition (part-by-part)</div>");
        parts.AppendLine(@"    <ul style=""margin:0; padding-left:18px;"">");

        foreach (var kv in l.Condition.Parts)
            parts.AppendLine($@"      <li><strong>{Html.Escape(Cap(kv.Key))}:</strong> {Html.Escape(kv.Value)}</li>");

        if (!string.IsNullOrWhiteSpace(l.Condition.Note))
            parts.AppendLine($@"      <li><strong>Note:</strong> {Html.Escape(l.Condition.Note!)}</li>");

        parts.AppendLine(@"    </ul>");
        parts.AppendLine(@"  </div>");
        parts.AppendLine(@"</div>");

        return parts.ToString();
    }

    private static string Section(string title, string bodyHtml)
        => $@"
<div style=""margin-top:18px;"">
  <div style=""font-size:16px; font-weight:800; border-bottom:1px solid #eee; padding-bottom:6px;"">{Html.Escape(title)}</div>
  {bodyHtml}
</div>";

    private static string Ul(List<string> liItems)
        => $@"<ul style=""margin:10px 0 0 18px; padding:0;"">
{string.Join("", liItems)}
</ul>";

    private static string LiStrong(string label, string value)
        => $"<li><strong>{Html.Escape(label)}:</strong> {Html.Escape(value)}</li>";

    private static string Cap(string s)
        => string.IsNullOrWhiteSpace(s) ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();
}
